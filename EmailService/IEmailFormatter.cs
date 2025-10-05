using JobSearcher.Job;
using JobSearcher.JobOpening;

namespace JobSearch.Emails
{
    public interface IEmailReportFormatter
    {
        string FormatReport(IDictionary<Site, List<JobInfo>> resultsBySite, int userId);
    }
}