using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobSearcher.Report
{
    public class ReportSetupBackgroundService : BackgroundService
    {
        private readonly ILogger<ReportSetupBackgroundService> _logger;
        private readonly IServiceProvider _services;

        private int _currentLowerBound = 1;
        private readonly object _lock = new object();
        private const int ChunkSize = 1000;

        public ReportSetupBackgroundService(ILogger<ReportSetupBackgroundService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Report setup background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
                    var generateReportService = scope.ServiceProvider.GetRequiredService<IGenerateReportService>();

                    await CreateReportSetupsAsync(reportRepository, generateReportService, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during report setup execution.");
                }

                var now = DateTime.UtcNow;
                var nextHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
                var delay = nextHour - now;

                _logger.LogInformation("Waiting {DelayMinutes} minutes until next execution at {NextHour}.", delay.TotalMinutes, nextHour);

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task CreateReportSetupsAsync(IReportRepository reportRepository,
                                                    IGenerateReportService generateReportService,
                                                    CancellationToken stoppingToken)
        {
            var totalSchedules = await reportRepository.GetSchedulesAmount();
            if (totalSchedules == 0) return;

            int threadCount = (int)Math.Ceiling((double)totalSchedules / ChunkSize);

            for (int t = 0; t < threadCount; t++)
            {
                int lowerBound, upperBound;

                lock (_lock)
                {
                    lowerBound = _currentLowerBound;
                    upperBound = Math.Min(lowerBound + ChunkSize - 1, totalSchedules);
                    _currentLowerBound = upperBound + 1;

                    if (_currentLowerBound >= totalSchedules)
                        _currentLowerBound = 0;
                }

                var schedules = await reportRepository.GetSchedulesBetweenIds(lowerBound, upperBound);
                foreach (var schedule in schedules)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var tz = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZoneId);
                    var nowInTz = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
                   
                    foreach (var reportTime in schedule.ReportTimes)
                    {
                        if (reportTime.LocalTime.Hours == nowInTz.Hour)
                        {
                            await generateReportService.GenerateReportForUser(schedule.UserId);
                        }
                    }
                }
            }
        }
    }
}
