using JobSearcher.Account;
using JobSearcher.Report;
using Microsoft.AspNetCore.Mvc;

public class ReportSchedulerViewComponent : ViewComponent
{
    private readonly IReportRepository _reportRepository;
    private readonly IAccount _account;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<ReportSchedulerViewComponent> _logger;

    public ReportSchedulerViewComponent(
        IReportRepository reportRepository,
        IAccount account,
        IHttpContextAccessor http,
        ILogger<ReportSchedulerViewComponent> logger)
    {
        _reportRepository = reportRepository;
        _account = account;
        _http = http;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var user = await UserHelper.GetCurrentUserAsync(_http.HttpContext!, _account);


            var schedule = await _reportRepository.GetScheduleAsync(user!.Id);
       
            return View(schedule); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ReportScheduler view component");
            return Content("Error loading report scheduler");
        }
    }
}
