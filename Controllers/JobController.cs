using Microsoft.AspNetCore.Mvc;
using JobSearcher.Job;
using JobSearcher.JobOpening;

namespace JobSearcher.Controllers;

public class JobController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDictionary<Site, IJobSearcherAdapter> _searcherAdapters = new Dictionary<Site, IJobSearcherAdapter>();

    public JobController(ILogger<HomeController> logger, GlassDoorJobSearchAdapter glassDoorJobSearcherAdapter,
    IndeedJobSearcherAdapter indeedJobSearcherAdapter, PracujPlSearchAdapter pracujPlSearchAdapter)
    {
        _logger = logger;
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

            return PartialView("JobOpening/_JobOpeningList", jobs);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error getting glass door jobs {e}");
            return StatusCode(500, new { error = "Unexpected error try again" });
        }
    }


}