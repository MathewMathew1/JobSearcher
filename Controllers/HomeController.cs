using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JobSearcher.Models;
using JobSearch.Emails;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace JobSearcher.Controllers;


public class MessageDto : IValidatableObject
{
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 10,
        ErrorMessage = "Message must be between 10 and 2000 characters.")]
    public required string Message { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(100, ErrorMessage = "Subject must be under 100 characters.")]
    public required string Subject { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (Message.Contains("<script>", StringComparison.OrdinalIgnoreCase))
            yield return new ValidationResult("Potentially unsafe content detected in message.",
                new[] { nameof(Message) });

        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(Email, emailPattern))
            yield return new ValidationResult("Email format is invalid.", new[] { nameof(Email) });

        string[] blockedDomains = { "mailinator.com", "tempmail.com" };
        var domain = Email.Split('@').LastOrDefault()?.ToLowerInvariant();
        if (blockedDomains.Contains(domain))
            yield return new ValidationResult("Disposable email domains are not allowed.",
                new[] { nameof(Email) });
    }
}


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailReportFormatter _emailReportFormatter;
    private readonly IEmailService _emailService;
    private readonly string _emailOfCreator = "mateusz.lu.work@gmail.com";

    public HomeController(ILogger<HomeController> logger, IEmailReportFormatter emailReportFormatter, IEmailService emailService)
    {
        _logger = logger;
        _emailReportFormatter = emailReportFormatter;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string htmlBody = _emailReportFormatter.FormatUserMessage(message);
            await _emailService.SendEmailAsync(_emailOfCreator,
                message.Subject,
                htmlBody);

            return Ok("Message sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending feedback message");
            return StatusCode(500, "Error sending message.");
        }
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
