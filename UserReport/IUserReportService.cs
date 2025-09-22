using System.Collections.Concurrent;
using JobSearcher.Job;
using JobSearcher.JobOpening;

namespace JobSearcher.UserReport
{
    public interface IUserReportService
    {
        public Task<UserReportModel> AddUserReport(ConcurrentDictionary<Site, List<JobInfo>> resultsBySite, int userId);
        Task<int> GetUnseenReportsCount(int userId);
        Task MarkReportAsSeen(int reportId, int userId);
    }
}