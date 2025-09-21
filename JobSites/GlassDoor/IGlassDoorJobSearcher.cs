namespace JobSearcher.Job
{
    public interface IGlassDoorJobSearcher
    {
        Task<List<JobInfo>> GetJobOfferings(GlassDoorJobSearch search, int maxAmount = 20);
    }
}