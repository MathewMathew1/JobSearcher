using JobSearcher.JobOpening;

namespace JobSearcher.Job
{
    public class GlassDoorJobSearchAdapter : IJobSearcherAdapter
    {
        private readonly ILogger<GlassDoorJobSearchAdapter> _logger;
        public Site Site => Site.GlassDoor;
        private readonly GlassDoorJobSearcher _glassDoorJobSearcher;

        public GlassDoorJobSearchAdapter(ILogger<GlassDoorJobSearchAdapter> logger, GlassDoorJobSearcher glassDoorJobSearcher)
        {
            _logger = logger;
            _glassDoorJobSearcher = glassDoorJobSearcher;
        }

        public async Task<List<JobInfo>> GetJobOfferings(JobOpeningSearcherModel search, SearchedLink searchedLinks)
        {
            GlassDoorSearchModel searchModel = new GlassDoorSearchModel { JobSearched = search.JobSearched, Location = search.Location };
            return await _glassDoorJobSearcher.GetJobOfferings(searchModel, searchedLinks);
        }

    }
}