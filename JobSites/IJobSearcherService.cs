namespace JobSearcher.Job
{
    public interface IJobSearcherService
    {
        Task<List<JobInfo>> GetJobOfferings(JobSearchModel search, SearchedLink searchedLinks, int maxAmount = 20);
    }
}