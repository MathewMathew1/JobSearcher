using Microsoft.AspNetCore.Mvc;
using JobSearcher.Job;

namespace JobSearcher.Controllers;

public class JobController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GlassDoorJobSearcher _glassDoorJobSearcher;

    public JobController(ILogger<HomeController> logger, GlassDoorJobSearcher glassDoorJobSearcher)
    {
        _logger = logger;
        _glassDoorJobSearcher = glassDoorJobSearcher;
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
    public async Task<IActionResult> GetGlassDoorJobs(JobSearchModel search)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var jobs = await _glassDoorJobSearcher.GetJobOfferings(search, new SearchedLink());

            return PartialView("JobOpening/_JobOpeningList", jobs);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error getting glass door jobs {e}");
            return StatusCode(500, new { error = "Unexpected error try again" });
        }
    }

}