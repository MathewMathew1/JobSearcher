using JobSearcher.Account;
using JobSearcher.Api.MiddleWare;
using JobSearcher.Cv;
using Microsoft.AspNetCore.Mvc;


namespace JobSearcher.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ICvStorageService _cvStorage;
        private readonly ILogger<ProfileController> _logger;
        private readonly IUserCvStorageService _userCvStorage;
        private readonly IHttpContextAccessor _http;
        private readonly IAccount _account;

        public ProfileController(ICvStorageService cvStorage, ILogger<ProfileController> logger, IUserCvStorageService userCvStorage,
         IHttpContextAccessor http, IAccount account)
        {
            _cvStorage = cvStorage;
            _logger = logger;
            _userCvStorage = userCvStorage;
            _http = http;
            _account = account;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadCv(IFormFile file)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (file == null) return BadRequest();

                if (file.ContentType != "application/pdf" &&
                   file.ContentType != "application/msword" &&
                   file.ContentType != "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    return BadRequest("Only PDF and Word documents are allowed.");
                }

                if (user.UserCvs.Count > 0)
                {
                    return BadRequest("You can only upload one CV at a time.");
                }

                using var stream = file.OpenReadStream();
                var key = await _cvStorage.UploadCvAsync(stream, file.FileName, file.ContentType);
                var savedInDatabase = await _userCvStorage.UploadCvAsync(user.Id, key);

                if (!savedInDatabase)
                {
                    await _cvStorage.DeleteCvAsync(key);
                    return StatusCode(500, "Error saving CV information.");
                }

                return Ok(new { Key = key });
            }
            catch (Exception e)
            {
                _logger.LogError($"Error inserting user cv {e}");
                return StatusCode(500, new { error = "Unexpected error inserting user cv." });
            }
        }

        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpdateCv(IFormFile file)
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);
                if (file == null) return BadRequest();

                if (file.ContentType != "application/pdf" &&
                   file.ContentType != "application/msword" &&
                   file.ContentType != "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    return BadRequest("Only PDF and Word documents are allowed.");
                }

                if (user.UserCvs.Count == 0)
                {
                    return BadRequest("No existing CV to update. Please upload a CV first.");
                }

                using var stream = file.OpenReadStream();
                var key = await _cvStorage.UploadCvAsync(stream, file.FileName, file.ContentType);
                await _userCvStorage.UpdateFileNameKeyAsync(user.Id, key);

                return Ok(new { Key = key });
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating user cv: {e}");
                return StatusCode(500, new { error = "Unexpected error updating cv." });
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteCv()
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);

                await _cvStorage.DeleteCvAsync(user.UserCvs.ToList()[0].AwsS3Key);
                await _userCvStorage.DeleteCvAsync(user.UserCvs.ToList()[0].AwsS3Key, user.Id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error deleting user cv: {e}");
                return StatusCode(500, new { error = "Unexpected error deleting cv." });
            }

        }
    }
}

