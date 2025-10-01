using JobSearcher.JobOpening;

namespace JobSearcher.Job
{
    public class PracujPlSearchAdapter : IJobSearcherAdapter
    {
        private readonly PracujJobSearcher _pracujPlJobSearcher;

        public Site Site => Site.Indeed;

        public PracujPlSearchAdapter(PracujJobSearcher indeedJobSearcher)
        {
            _pracujPlJobSearcher = indeedJobSearcher;
        }

        public async Task<List<JobInfo>> GetJobOfferings(JobOpeningSearcherModel search, SearchedLink searchedLinks)
        {
            var model = new PracujPlSearchModel
            {
                JobSearched = search.JobSearched,
                Location = search.Location,
            };

            return await _pracujPlJobSearcher.GetJobOfferings(model, searchedLinks);
        }
    }
}
