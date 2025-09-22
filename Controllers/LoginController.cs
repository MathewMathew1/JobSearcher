using System.Security.Claims;
using JobSearcher.Account;
using JobSearcher.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace JobSearcher.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IAccount _account;
        private readonly IJwtService _jwtService;

        public LoginController(ILogger<LoginController> logger, IAccount account, IJwtService jwtService)
        {
            _logger = logger;
            _account = account;
            _jwtService = jwtService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("LoginCallback", "Login", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> LoginCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!result.Succeeded)
                {
                    return RedirectToAction("Login", "Login");
                }

                var principal = result.Principal;
                if (principal == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                var user = new User
                {
                    Name = principal.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
                    Email = principal.FindFirstValue(ClaimTypes.Email) ?? "Unknown",
                    ProfilePicture = principal.FindFirst("picture")?.Value ?? string.Empty
                };



                var userInDb = await _account.GetUser(user.Email);

                string jwtToken;
                if (userInDb == null)
                {
                    var userCreated = await _account.SetUser(user);
                    jwtToken = _jwtService.GenerateToken(userCreated.Id);
                    HttpContext.Items["UserId"] = userCreated.Id;
                    HttpContext.Items["CurrentUser"] = userCreated;
                }
                else
                {
                    jwtToken = _jwtService.GenerateToken(userInDb.Id);
                    HttpContext.Items["UserId"] = userInDb.Id;
                    HttpContext.Items["CurrentUser"] = userInDb;
                }

                Response.Cookies.Append("jwt_token", jwtToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });



                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication.");
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Items["UserId"] = null;
                HttpContext.Items["CurrentUser"] = null;

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                };

                Response.Cookies.Delete("jwt_token", cookieOptions);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Logout");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

    }
}