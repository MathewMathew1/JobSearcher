using JobSearcher.Account;
using JobSearcher.Api.MiddleWare;
using JobSearcher.Report;
using Microsoft.AspNetCore.Mvc;

namespace UserJobSearcher.Controllers
{
    [Route("user/report")]
    public class UserReportController : Controller
    {
        private readonly ILogger<UserReportController> _logger;
        private readonly IReportRepository _reportRepository;
        private readonly IAccount _account;
        private readonly IHttpContextAccessor _http;

        public UserReportController(
            ILogger<UserReportController> logger,
            IReportRepository reportRepository,
            IAccount account,
            IHttpContextAccessor http)
        {
            _logger = logger;
            _reportRepository = reportRepository;
            _account = account;
            _http = http;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (user == null)
                    return RedirectToAction("Login", "Home");

                var schedule = await _reportRepository.GetScheduleAsync(user.Id);
                return View(schedule); 
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving user report schedule");
                return StatusCode(500, new { error = "Unexpected error retrieving schedule." });
            }
        }


        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSchedule(int userId)
        {
            try
            {
                var schedule = await _reportRepository.GetScheduleAsync(userId);
                if (schedule == null)
                    return NotFound(new { error = "Schedule not found." });

                return Json(schedule);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving schedule for user {UserId}", userId);
                return StatusCode(500, new { error = "Unexpected error retrieving schedule." });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] string timeZoneId)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (user == null)
                    return Unauthorized();

                var schedule = await _reportRepository.CreateScheduleAsync(user.Id, timeZoneId);
                return Json(schedule);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating report schedule");
                return StatusCode(500, new { error = "Unexpected error creating schedule." });
            }
        }

        [Authorize]
        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromBody] UpdateReportScheduleDto dto)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (user == null)
                    return Unauthorized();

                var updated = await _reportRepository.UpdateScheduleAsync(user.Id, dto.TimeZoneId, dto.IsActive);
                return Json(updated);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating report schedule");
                return StatusCode(500, new { error = "Unexpected error updating schedule." });
            }
        }


        [Authorize]
        [HttpPost("times")]
        public async Task<IActionResult> SetReportTimes([FromBody] List<ReportCreateDto> times)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (user == null)
                    return Unauthorized();

                await _reportRepository.CreateReportTimes(times, user.Id);
                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting report times");
                return StatusCode(500, new { error = "Unexpected error setting report times." });
            }
        }
    }

    public class UpdateReportScheduleDto
    {
        public string? TimeZoneId { get; set; }
        public bool? IsActive { get; set; }
    }
}
