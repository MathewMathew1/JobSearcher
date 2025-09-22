namespace JobSearcher.Report
{
    public interface IReportRepository
    {
        Task CreateReportTimes(List<ReportCreateDto> reportTimes, int userId);
        Task<UserReportSchedule> CreateScheduleAsync(int userId, UserReportScheduleCreateDto userReportScheduleCreateDto);
        Task<UserReportSchedule> UpdateScheduleAsync(int userId, UserReportScheduleCreateDto userReportScheduleCreateDto);
        Task<UserReportSchedule?> GetScheduleAsync(int userId);
        Task<int> GetSchedulesAmount();
        Task<List<UserReportSchedule>> GetSchedulesBetweenIds(int lowerBoundId, int upperBound);
        Task<int?> GetHighestScheduleIdAsync();
    }
}