using JobSearcher.Data;
using Microsoft.EntityFrameworkCore;

namespace JobSearcher.Report
{
    public class ReportRepository : IReportRepository
    {
        private readonly ILogger<ReportRepository> _logger;
        private readonly AppDbContext _appDbContext;

        public ReportRepository(ILogger<ReportRepository> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public async Task CreateReportTimes(List<ReportCreateDto> reportTimes, int userId)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {

                var existing = _appDbContext.ReportTimes
                .Where(rt => rt.UserReportSchedule.UserId == userId);

                var schedule = await _appDbContext.UserReportSchedules
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (schedule == null)
                {
                    throw new InvalidOperationException("User report schedule not found.");
                }

                _appDbContext.ReportTimes.RemoveRange(existing);

                var newTimes = reportTimes.Select(dto => new ReportTime
                {
                    LocalTime = dto.LocalTime,
                    UserReportScheduleId = schedule.Id
                });

                _appDbContext.ReportTimes.AddRange(newTimes);

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating report times for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserReportSchedule> CreateScheduleAsync(int userId, UserReportScheduleCreateDto userReportScheduleCreateDto)
        {
            var existing = await _appDbContext.UserReportSchedules
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existing != null)
            {
                throw new InvalidOperationException("User schedule already exists.");
            }

            var schedule = new UserReportSchedule
            {
                UserId = userId,
                TimeZoneId = userReportScheduleCreateDto.TimeZoneId,
                IsActive = userReportScheduleCreateDto.IsActive
            };

            _appDbContext.UserReportSchedules.Add(schedule);
            await _appDbContext.SaveChangesAsync();

            return schedule;
        }

        public async Task<UserReportSchedule> UpdateScheduleAsync(int userId, UserReportScheduleCreateDto userReportScheduleCreateDto)
        {
            var schedule = await _appDbContext.UserReportSchedules
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (schedule == null)
            {
                throw new InvalidOperationException("User schedule not found.");
            }

  
            schedule.TimeZoneId = userReportScheduleCreateDto.TimeZoneId;
            schedule.IsActive = userReportScheduleCreateDto.IsActive;
            

            _appDbContext.UserReportSchedules.Update(schedule);
            await _appDbContext.SaveChangesAsync();

            return schedule;
        }

        public async Task<UserReportSchedule?> GetScheduleAsync(int userId)
        {
            var schedule = await _appDbContext.UserReportSchedules
                .Include(s => s.ReportTimes) 
                .FirstOrDefaultAsync(s => s.UserId == userId);
            return schedule;
        }
    }
}