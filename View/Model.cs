using JobSearcher.Account;
using JobSearcher.UserReport;
using JobSearcher.JobOpening;
using JobSearcher.Job;
using System.Collections.Concurrent;


namespace JobSearcher.ViewModels
{

    public class UserReportsViewModel
    {
        public UserInDatabase User { get; set; } = null!;
        public List<(UserReportModel Report, ConcurrentDictionary<Site, List<JobInfo>> Data)> Reports { get; set; } = new();
    }
}