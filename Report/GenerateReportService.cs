using System.Collections.Concurrent;
using JobSearch.Emails;
using JobSearcher.Account;
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

        public GenerateReportService(ILogger<GenerateReportService> logger, IUserReportService userReportService, IJobOpeningSearcher jobOpeningSearcher,
        GlassDoorJobSearchAdapter glassDoorJobSearcherAdapter, IUserFetchedLinkRepository userFetchedLinkRepository,
        IndeedJobSearcherAdapter indeedJobSearcherAdapter, PracujPlSearchAdapter pracujPlSearchAdapter,
        IEmailReportFormatter emailReportFormatter, IEmailService emailService, IAccount accountService)
        {
            _logger = logger;
            _userReportService = userReportService;
            _jobOpeningSearcher = jobOpeningSearcher;
            _emailReportFormatter = emailReportFormatter;
            _emailService = emailService;
            _accountService = accountService;

            _searcherAdapters.Add(Site.GlassDoor, glassDoorJobSearcherAdapter);
            _searcherAdapters.Add(Site.Indeed, indeedJobSearcherAdapter);
            _searcherAdapters.Add(Site.PracujPl, pracujPlSearchAdapter);
            _userFetchedLinkRepository = userFetchedLinkRepository;
        }

        public async Task GenerateReportForUser(int userId)
        {
            var searches = await _jobOpeningSearcher.GetSearchesByUser(userId);

            var resultsBySite = new ConcurrentDictionary<Site, List<JobInfo>>();

            var searchedLinks = await _userFetchedLinkRepository.GetAllLinksAsync(userId);
            SearchedLink searchedLink = new SearchedLink
            {
                SearchedInDatabase = searchedLinks,
                NewLinks = new ConcurrentBag<string>()
            };

            foreach (var search in searches.Where(s => s.IsActive))
            {
                if (!_searcherAdapters.TryGetValue(search.Site, out var service))
                {
                    _logger.LogWarning("No search service registered for site {Site}", search.Site);
                    continue;
                }

                var jobs = await service.GetJobOfferings(search, searchedLink);

                resultsBySite.AddOrUpdate(
                    search.Site,
                    jobs,
                    (_, existing) =>
                    {
                        existing.AddRange(jobs);
                        return existing;
                    });
            }
            _ = _userFetchedLinkRepository.SaveLinksAsync(userId, searchedLink.NewLinks);
          
            var htmlBody = _emailReportFormatter.FormatReport(resultsBySite, userId);

            var userData = await _accountService.GetEmailAndCvByUserId(userId);
            _  = _emailService.SendEmailAsync(
                userData.Value.Email,
                "Your Daily Job Report",
                htmlBody,
                "Please open the email in HTML-capable client."
            );
            await _userReportService.AddUserReport(resultsBySite, userId);

            _logger.LogInformation("Generated report for user {UserId}, {Sites} sites processed", userId, resultsBySite.Count);

        }
    }
}