using JobSearcher.Account;
using JobSearcher.Api.MiddleWare;
using JobSearcher.Report;
using Microsoft.AspNetCore.Mvc;

namespace UserJobSearcher.Controllers
{
    [Route("user/report")]
    public class ReportSchedulerController : Controller
    {
        private readonly ILogger<ReportSchedulerController> _logger;
        private readonly IReportRepository _reportRepository;
        private readonly IAccount _account;
        private readonly IHttpContextAccessor _http;

        public ReportSchedulerController(
            ILogger<ReportSchedulerController> logger,
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

                return StatusCode(500, new { error = "Unexpected error retrieving schedule." });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UserReportScheduleCreateDto userReportScheduleCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);


                var schedule = await _reportRepository.CreateScheduleAsync(user.Id, userReportScheduleCreateDto);
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
        public async Task<IActionResult> Update([FromBody] UserReportScheduleCreateDto userReportScheduleCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
      

                var updated = await _reportRepository.UpdateScheduleAsync(user.Id, userReportScheduleCreateDto);
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


}
