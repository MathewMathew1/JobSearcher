using JobSearcher.Account;
using Microsoft.AspNetCore.Mvc;

public class NavbarViewComponent : ViewComponent
{
    private readonly IAccount _account;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<NavbarViewComponent> _logger;

    public NavbarViewComponent(IAccount account, IHttpContextAccessor http, ILogger<NavbarViewComponent> logger)
    {
        _account = account;
        _http = http;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {

        var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);

        return View(user); 
    }
}
