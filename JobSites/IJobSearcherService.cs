namespace JobSearcher.Job
{
    public interface IJobSearcherService
    {
        Task<List<JobInfo>> GetJobOfferings(JobSearchModel search, int maxAmount = 20);
    }
}