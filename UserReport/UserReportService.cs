using System.Collections.Concurrent;
using System.Text.Json;
using JobSearcher.Data;
using JobSearcher.Job;
using JobSearcher.JobOpening;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSearcher.UserReport
{
    public class UserReportService : IUserReportService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UserReportService> _logger;

        public UserReportService(AppDbContext dbContext, ILogger<UserReportService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UserReportModel> AddUserReport(ConcurrentDictionary<Site, List<JobInfo>> resultsBySite, int userId)
        {
            if (resultsBySite == null || resultsBySite.IsEmpty)
                throw new ArgumentException("No job results to store.", nameof(resultsBySite));

            var allJobsJson = JsonSerializer.Serialize(resultsBySite);

            var report = new UserReportModel
            {
                UserId = userId,
                DataJson = allJobsJson,
                CreatedAt = DateTime.UtcNow,
                SeenByUser = false
            };

            await _dbContext.UserReports.AddAsync(report);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Added a single consolidated report for user {UserId}", userId);

            return report;
        }


        public async Task<int> GetUnseenReportsCount(int userId)
        {
            return await _dbContext.UserReports
                .Where(r => r.UserId == userId && !r.SeenByUser)
                .CountAsync();
        }

        public async Task MarkReportAsSeen(int reportId, int userId)
        {
            var report = await _dbContext.UserReports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.UserId == userId);

            if (report == null)
            {
                _logger.LogWarning("Report {ReportId} not found for user {UserId}", reportId, userId);
                return;
            }

            report.SeenByUser = true;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Report {ReportId} marked as seen by user {UserId}", reportId, userId);
        }
    }

}
