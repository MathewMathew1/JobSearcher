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

                _logger.LogInformation("User {Name} with email {Email} logged in.", user.Name, user.Email);

                var userInDb = await _account.GetUser(user.Email);

                string jwtToken;
                if (userInDb == null)
                {
                    var userCreated = await _account.SetUser(user);
                    jwtToken = _jwtService.GenerateToken(userCreated.Id);
                }
                else
                {
                    jwtToken = _jwtService.GenerateToken(userInDb.Id);
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

    }
}