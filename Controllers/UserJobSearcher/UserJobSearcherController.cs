using JobSearcher.Account;
using JobSearcher.Api.MiddleWare;
using JobSearcher.JobOpening;
using Microsoft.AspNetCore.Mvc;

namespace UserJobSearcher.Controllers
{
    [Route("user/jobSearcher")]
    public class UserJobSearcherController : Controller
    {
        private readonly ILogger<UserJobSearcherController> _logger;
        private readonly IJobOpeningSearcher _searcher;
        private readonly IAccount _account;
        private readonly IHttpContextAccessor _http;

        public UserJobSearcherController(
            ILogger<UserJobSearcherController> logger,
            IJobOpeningSearcher searcher,
            IAccount account,
            IHttpContextAccessor http)
        {
            _logger = logger;
            _searcher = searcher;
            _account = account;
            _http = http;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (user == null)
                {
                    return RedirectToAction("Login", "Home");
                }
                _logger.LogInformation($"User {user.Email} has {user.UserSearches.Count} searches.");
                foreach (var search in user.UserSearches)
                {
                    _logger.LogInformation($"User Search: {search.JobSearched} in {search.Location} on {search.Site} and {search.CountryCode}");
                }
                return View(user.UserSearches);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error retrieving user searches: {e}");
                return StatusCode(500, new { error = "Unexpected error retrieving searches." });
            }
        }

        [Authorize]
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(SearchInfo searchInfo)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (user == null)
                    return Unauthorized();

                if (user.UserSearches.Count >= 3)
                {
                    return BadRequest(new { error = "Maximum of 3 searches allowed for this site." });
                }

                var created = await _searcher.CreateJobOpeningSearch(searchInfo, user.Id);

                return Json(created);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error creating search: {e}");
                return StatusCode(500, new { error = "Unexpected error" });
            }
        }


        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> Update(int id, SearchInfo searchInfo)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                _logger.LogInformation($"Update {searchInfo.CountryCode}");
                var updated = await _searcher.UpdateJobOpeningSearch(searchInfo, id, user.Id);
                return updated == null
                    ? NotFound(new { error = "Search not found." })
                    : Json(updated);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating search: {e}");
                return StatusCode(500, new { error = "Unexpected error updating search." });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account)!;

                await _searcher.DeleteSearchById(id, user.Id);
                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                _logger.LogError($"Error deleting search: {e}");
                return StatusCode(500, new { error = "Unexpected error deleting search." });
            }
        }


    }
}
