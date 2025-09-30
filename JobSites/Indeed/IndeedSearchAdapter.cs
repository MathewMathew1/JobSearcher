using JobSearcher.JobOpening;

namespace JobSearcher.Job
{
    public class IndeedJobSearcherAdapter : IJobSearcherAdapter
    {
        private readonly IndeedJobSearcher _indeedJobSearcher;

        public Site Site => Site.Indeed;

        public IndeedJobSearcherAdapter(IndeedJobSearcher indeedJobSearcher)
        {
            _indeedJobSearcher = indeedJobSearcher;
        }

        public async Task<List<JobInfo>> GetJobOfferings(JobOpeningSearcherModel search, SearchedLink searchedLinks)
        {
            var model = new IndeedSearchModel
            {
                JobSearched = search.JobSearched,
                Location = search.Location,
                CountryCode = search.CountryCode ?? "www.indeed.com",
                Sort = search.Sort
            };

            return await _indeedJobSearcher.GetJobOfferings(model, searchedLinks);
        }
    }
}
