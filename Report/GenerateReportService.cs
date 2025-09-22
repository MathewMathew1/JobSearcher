using System.Collections.Concurrent;
using JobSearcher.Job;
using JobSearcher.JobOpening;

namespace JobSearcher.Report
{
    public class GenerateReportService : IGenerateReportService
    {
        private readonly ILogger<GenerateReportService> _logger;
        private readonly IReportRepository _reportRepository;
        private readonly IJobOpeningSearcher _jobOpeningSearcher;
        private readonly IDictionary<Site, IJobSearcherService> _searcherServices = new Dictionary<Site, IJobSearcherService>();

        public GenerateReportService(ILogger<GenerateReportService> logger, IReportRepository reportRepository, IJobOpeningSearcher jobOpeningSearcher, GlassDoorJobSearcher glassDoorJobSearcher)
        {
            _logger = logger;
            _reportRepository = reportRepository;
            _jobOpeningSearcher = jobOpeningSearcher;

            _searcherServices.Add(Site.GlassDoor, glassDoorJobSearcher);
        }

        public async Task GenerateReportForUser(int userId)
        {
            var searches = await _jobOpeningSearcher.GetSearchesByUser(userId);

            var resultsBySite = new ConcurrentDictionary<Site, List<JobInfo>>();

            foreach (var search in searches.Where(s => s.IsActive))
            {
                if (!_searcherServices.TryGetValue(search.Site, out var service))
                {
                    _logger.LogWarning("No search service registered for site {Site}", search.Site);
                    continue;
                }

                var jobs = await service.GetJobOfferings(new JobSearchModel
                {
                    JobSearched = search.JobSearched,
                    Location = search.Location
                });

                resultsBySite.AddOrUpdate(
                    search.Site,
                    jobs,
                    (_, existing) =>
                    {
                        existing.AddRange(jobs);
                        return existing;
                    });
            }

 
            _logger.LogInformation("Generated report for user {UserId}, {Sites} sites processed", userId, resultsBySite.Count);
 
        }
    }
}