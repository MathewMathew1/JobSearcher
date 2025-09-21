using JobSearcher.Account;
using JobSearcher.Api.MiddleWare;
using Microsoft.AspNetCore.Mvc;

namespace JobSearcher.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IAccount _account;

        public UserController(ILogger<UserController> logger, IAccount account)
        {
            _logger = logger;
            _account = account;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userId = (int)Request.HttpContext.Items["UserId"]!;
                UserInDatabase? user = await _account.GetUserById(userId);

                if (user == null)
                {
                    return StatusCode(500, new { error = "Unexpected error while getting user" });
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while getting user info {e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }


        }

    }
}