using Microsoft.AspNetCore.Mvc;
using JobSearcher.UserReport;
using JobSearcher.Account;

namespace UserJobSearcher.Controllers
{
    [Route("[controller]")]
    public class UserReportController : Controller
    {
        private readonly IUserReportService _reportRepository;
        private readonly IAccount _account;
        private readonly IHttpContextAccessor _http;

        public UserReportController(IUserReportService reportRepository, IAccount account, IHttpContextAccessor http)
        {
            _reportRepository = reportRepository;
            _account = account;
            _http = http;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var reports = await _reportRepository.GetUserReportsAsync(user.Id);
            return View(reports);
        }

        [HttpPost("MarkSeen")]
        public async Task<IActionResult> MarkSeen(int id)
        {
            var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
            await _reportRepository.MarkReportAsSeen(id, user.Id);

            return Ok();
        }

    }
}
