namespace JobSearcher.Report
{
    public interface IReportRepository
    {
        Task CreateReportTimes(List<ReportCreateDto> reportTimes, int userId);
        Task<UserReportSchedule> CreateScheduleAsync(int userId, string timeZoneId);
        Task<UserReportSchedule> UpdateScheduleAsync(int userId, string? timeZoneId = null, bool? isActive = null);
        Task<UserReportSchedule?> GetScheduleAsync(int userId);
    }
}