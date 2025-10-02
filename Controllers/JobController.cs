using Microsoft.AspNetCore.Mvc;
using JobSearcher.Job;
using JobSearcher.JobOpening;
using JobSearcher.Account;
using JobSearcher.Cv;

namespace JobSearcher.Controllers
{

    public class JobSearchResultViewModel
    {
        public List<JobInfo> Jobs { get; set; } = new();
        public UserInDatabase? User { get; set; }
    }

    public class JobWithUserViewModel
    {
        public JobInfo Job { get; set; } = null!;
        public UserInDatabase? User { get; set; }
    }

    public class JobController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDictionary<Site, IJobSearcherAdapter> _searcherAdapters = new Dictionary<Site, IJobSearcherAdapter>();
        private readonly IHttpContextAccessor _http;
        private readonly IAccount _account;
        private readonly ICvStorageService _cvStorage;
        private readonly ICvParserService _cvParser;

        public JobController(ILogger<HomeController> logger, GlassDoorJobSearchAdapter glassDoorJobSearcherAdapter,
        IndeedJobSearcherAdapter indeedJobSearcherAdapter, PracujPlSearchAdapter pracujPlSearchAdapter,
        IHttpContextAccessor http, IAccount account, ICvStorageService cvStorage, ICvParserService cvParser)
        {
            _http = http;
            _account = account;
            _logger = logger;
            _cvStorage = cvStorage;
            _cvParser = cvParser;
            _searcherAdapters.Add(Site.GlassDoor, glassDoorJobSearcherAdapter);
            _searcherAdapters.Add(Site.Indeed, indeedJobSearcherAdapter);
            _searcherAdapters.Add(Site.PracujPl, pracujPlSearchAdapter);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchJobs(JobOpeningSearcherModel search)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                _searcherAdapters.TryGetValue(search.Site, out var adapter);
                if (adapter == null)
                {
                    return BadRequest(new { error = "No search service registered for site" });
                }

                var jobs = await adapter.GetJobOfferings(search, new SearchedLink());

                if (jobs.Count == 0)
                {
                    return PartialView("JobOpening/_NoJobResults");
                }

                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                var vm = new JobSearchResultViewModel
                {
                    Jobs = jobs,
                    User = user
                };

                return PartialView("JobOpening/_JobOpeningList", vm);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error getting glass door jobs {e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeJob([FromForm] string jobName, [FromForm] string jobDescription, IFormFile? cvFile)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (user == null)
                    return Unauthorized();

                string? cvFilename = null;
                string cvContent = "";

                if (cvFile != null)
                {
                    cvFilename = cvFile.FileName;

                    using var inputStream = cvFile.OpenReadStream();
                    using var seekableStream = new MemoryStream();
                    await inputStream.CopyToAsync(seekableStream);
                    seekableStream.Position = 0;
                    cvContent = _cvParser.ExtractText(seekableStream, cvFilename);
                }
                else if (user.UserCv != null)
                {
                    cvFilename = user.UserCv.Filename;

                    using var inputStream = await _cvStorage.DownloadCvAsync(user.UserCv.AwsS3Key);
                    using var seekableStream = new MemoryStream();
                    await inputStream.CopyToAsync(seekableStream);
                    seekableStream.Position = 0;
                    cvContent = _cvParser.ExtractText(seekableStream, cvFilename);
                }


                if (cvContent == null)
                {
                    return BadRequest(new { error = "No CV available to analyze." });
                }

                var job = new { Name = jobName };
                _logger.LogInformation($"Analyzing job {jobName} with CV {cvFilename}, size {cvContent} bytes");
                return Json(new
                {
                    Job = job,
                    CvFilename = cvFilename,
                    Message = "Analysis not implemented yet"
                });
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Error occured during analysis {e}");
                return StatusCode(500, new { error = "Unexpected error during analysis", details = e.Message });
            }
        }


    }
}