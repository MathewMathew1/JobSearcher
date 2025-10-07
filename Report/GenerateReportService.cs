using System.Collections.Concurrent;
using JobSearch.Emails;
using JobSearch.Utils;
using JobSearcher.Account;
using JobSearcher.AiAnalyzer;
using JobSearcher.Job;
using JobSearcher.JobOpening;
using JobSearcher.UserReport;
using JobSearcher.UserSearchLink;

namespace JobSearcher.Report
{
    public class GenerateReportService : IGenerateReportService
    {
        private readonly ILogger<GenerateReportService> _logger;
        private readonly IUserReportService _userReportService;
        private readonly IJobOpeningSearcher _jobOpeningSearcher;
        private readonly IUserFetchedLinkRepository _userFetchedLinkRepository;
        private readonly IDictionary<Site, IJobSearcherAdapter> _searcherAdapters = new Dictionary<Site, IJobSearcherAdapter>();
        private readonly IEmailReportFormatter _emailReportFormatter;
        private readonly IEmailService _emailService;
        private readonly IAccount _accountService;
        private readonly ICvUtils _cvUtils;
        private readonly IJobAnalyzerService _jobAnalyzeService;

        public GenerateReportService(ILogger<GenerateReportService> logger, IUserReportService userReportService, IJobOpeningSearcher jobOpeningSearcher,
        GlassDoorJobSearchAdapter glassDoorJobSearcherAdapter, IUserFetchedLinkRepository userFetchedLinkRepository,
        IndeedJobSearcherAdapter indeedJobSearcherAdapter, PracujPlSearchAdapter pracujPlSearchAdapter,
        IEmailReportFormatter emailReportFormatter, IEmailService emailService, IAccount accountService, ICvUtils cvUtils,
        IJobAnalyzerService jobAnalyzerService)
        {
            _logger = logger;
            _userReportService = userReportService;
            _jobOpeningSearcher = jobOpeningSearcher;
            _emailReportFormatter = emailReportFormatter;
            _emailService = emailService;
            _accountService = accountService;
            _jobAnalyzeService = jobAnalyzerService;
            _cvUtils = cvUtils;

            _searcherAdapters.Add(Site.GlassDoor, glassDoorJobSearcherAdapter);
            _searcherAdapters.Add(Site.Indeed, indeedJobSearcherAdapter);
            _searcherAdapters.Add(Site.PracujPl, pracujPlSearchAdapter);
            _userFetchedLinkRepository = userFetchedLinkRepository;
        }

        public async Task GenerateReportForUser(int userId, bool analyzeMatch)
        {

            var userData = await _accountService.GetEmailAndCvByUserId(userId);
            string? cvContent = null;
            if (userData.Value.UserCv != null)
                cvContent = await _cvUtils.GetContentOfCvByUserIdAsync(userData.Value.UserCv);


            var searches = await _jobOpeningSearcher.GetSearchesByUser(userId);
            var searchedLinks = await _userFetchedLinkRepository.GetAllLinksAsync(userId);


            var resultsBySite = new ConcurrentDictionary<Site, List<JobInfo>>();
            var searchedLink = new SearchedLink
            {
                SearchedInDatabase = searchedLinks,
                NewLinks = new ConcurrentBag<string>()
            };

            foreach (var search in searches.Where(s => s.IsActive))
            {
                if (!_searcherAdapters.TryGetValue(search.Site, out var service))
                    continue;

                var jobs = await service.GetJobOfferings(search, searchedLink);
                resultsBySite.AddOrUpdate(search.Site, jobs, (_, existing) =>
                {
                    existing.AddRange(jobs);
                    return existing;
                });
            }

            await _userFetchedLinkRepository.SaveLinksAsync(userId, searchedLink.NewLinks);

            if (cvContent != null && analyzeMatch)
            {
                var allJobs = resultsBySite.SelectMany(kv => kv.Value).ToList();
                var matchResults = await _jobAnalyzeService.AnalyzeJobDescriptionsAsync(
                    cvContent,
                    allJobs.Select(j => j.Description).ToList()
                );
                int i = 0;
                foreach (var job in allJobs)
                    job.MatchToUserCv = i < matchResults.Length ? matchResults[i++] : 0;
            }

            var htmlBody = _emailReportFormatter.FormatReport(resultsBySite, userId);
            await _emailService.SendEmailAsync(userData.Value.Email, "Your Daily Job Report", htmlBody,
                "Please open the email in HTML-capable client.");
            await _userReportService.AddUserReport(resultsBySite, userId);
        }


    }
}