namespace JobSearcher.Job
{
    public interface IJobSearcherService<TSearch> where TSearch : JobSearchModel
    {
        Task<List<JobInfo>> GetJobOfferings(TSearch search, SearchedLink searchedLinks, int maxAmount = 20);
    }
}