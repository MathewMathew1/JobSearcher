using Microsoft.Playwright;
using JobSearch.Utils;
using JobSearch.ProxyService;
using JobSearch.Sanitizer;

namespace JobSearcher.Job
{
    public class GlassDoorJobSearcher : IJobSearcherService<GlassDoorSearchModel>
    {
        private readonly object _lock = new();
        private readonly ILogger<GlassDoorJobSearcher> _logger;
        private readonly IProxyService _proxyService;
        private readonly ISanitizerService _sanitizeService;

        public GlassDoorJobSearcher(ILogger<GlassDoorJobSearcher> logger, IProxyService proxyService, ISanitizerService sanitizerService)
        {
            _logger = logger;
            _proxyService = proxyService;
            _sanitizeService = sanitizerService;
        }

        public async Task<List<JobInfo>> GetJobOfferings(GlassDoorSearchModel search, SearchedLink searchedLinks, int maxAmount = 10)
        {
            var jobs = new List<JobInfo>();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Args = new[] { "--ignore-certificate-errors", "--disable-blink-features=AutomationControlled" }

            });
            var proxy = _proxyService.GetContextOptions();
            var context = await browser.NewContextAsync(_proxyService.GetContextOptions());
            var page = await context.NewPageAsync();
            var url = $"https://www.glassdoor.com/Job/{search.Location}-{search.JobSearched}-jobs-SRCH_IL.0,6_IN193_KO7,16.htm?sortBy=date_desc";
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            int totalCollected = 0;

            while (totalCollected < maxAmount)
            {
                var jobNodes = await page.QuerySelectorAllAsync("li[data-test='jobListing']");
                if (jobNodes.Count == 0)
                {
                    Console.WriteLine("No job results found.");
                    break;
                }

                foreach (var jobNode in jobNodes)
                {
                    if (totalCollected >= maxAmount)
                        break;

                    await jobNode.ScrollIntoViewIfNeededAsync();
                    await page.WaitForTimeoutAsync(300);

                    var titleHandle = await jobNode.QuerySelectorAsync("a[data-test='job-title']");
                    var link = await titleHandle?.GetAttributeAsync("href");

                    var title = titleHandle != null ? await titleHandle.InnerTextAsync() : "Unknown";

                    if (string.IsNullOrEmpty(link))
                        continue;

                    link = link.StartsWith("http") ? link : "https://www.glassdoor.com" + link;
                    var normalizeLink = LinkHelper.NormalizeLink(link);

                    lock (_lock)
                    {
                        if (searchedLinks.SearchedInDatabase.Contains(normalizeLink) || searchedLinks.NewLinks.Contains(normalizeLink))
                        {
                            continue;
                        }

                        searchedLinks.NewLinks.Add(normalizeLink);
                    }

                    lock (_lock)
                    {
                        if (searchedLinks.SearchedInDatabase.Contains(link) || searchedLinks.NewLinks.Contains(link))
                            continue;
                        searchedLinks.NewLinks.Add(link);
                    }

                    var employer = await jobNode.QuerySelectorAsync("span.EmployerProfile_compactEmployerName") is IElementHandle e ? await e.InnerTextAsync() : null;
                    var location = await jobNode.QuerySelectorAsync("div[data-test='emp-location']") is IElementHandle l ? await l.InnerTextAsync() : null;
                    var salary = await jobNode.QuerySelectorAsync("div[data-test='detailSalary']") is IElementHandle s ? await s.InnerTextAsync() : null;
                    var img = await jobNode.QuerySelectorAsync("img.avatar-base_Image") is IElementHandle i ? await i.GetAttributeAsync("src") : null;


                    var extensiveDescription = await TryExpandAndExtractDescriptionAsync(page, jobNode);
                    jobs.Add(new JobInfo
                    {
                        Name = title,
                        Link = normalizeLink,
                        Description = $"{employer} | {location} | {salary}",
                        ExtensiveDescription = _sanitizeService.SanitizeHtmlDocument(extensiveDescription),
                        ImageLink = img
                    });

                    totalCollected++;
                }


                var seeMoreButton = await page.QuerySelectorAsync("button[data-test='seeMoreJobs']");
                if (seeMoreButton == null)
                    break;

                await seeMoreButton.ClickAsync();
                await page.WaitForTimeoutAsync(2000);
            }

            await browser.CloseAsync();
            return jobs;
        }

        private async Task<string> TryExpandAndExtractDescriptionAsync(IPage page, IElementHandle jobElement)
        {
            try
            {
                 await DealWithModal(page);
                await HumanScrollToElementAsync(page, jobElement);
                await HumanMoveAndClickAsync(page, jobElement);

                await DealWithModal(page);
                var section = await page.WaitForSelectorAsync(
                    "section.Section_sectionComponent__nRsB2 div.JobDetails_jobDescription__uW_fK",
                    new PageWaitForSelectorOptions
                    {
                        Timeout = 15000,
                        State = WaitForSelectorState.Attached
                    });

                if (section == null) return string.Empty;

                await page.WaitForTimeoutAsync(Random.Shared.Next(500, 1200)); 

                return await section.InnerHTMLAsync();
            }
            catch (TimeoutException)
            {
                _logger.LogError("Timeout: job description section did not appear in time.");
                return string.Empty;
            }
            catch (PlaywrightException ex)
            {
                _logger.LogError($"Playwright error while expanding job: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while expanding job: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task DealWithModal(IPage page)
        {
            var modal = await page.QuerySelectorAsync("div[data-test='authModalContainerV2-content']");
            if (modal != null)
            {
                _logger.LogInformation("Auth modal detected. Attempting to close.");

                var closeButton = await page.QuerySelectorAsync("button.CloseButton");
                if (closeButton != null)
                {
                    try
                    {
                        await closeButton.ClickAsync();
                        await page.WaitForTimeoutAsync(800); // wait for modal animation to finish
                    }
                    catch (PlaywrightException)
                    {
                        _logger.LogWarning("Close button click failed; removing modal via JS.");
                        await page.EvaluateAsync(
                            @"() => {
                            const modal = document.querySelector('div[data-test=""authModalContainerV2-content""]');
                            if (modal && modal.closest('dialog')) modal.closest('dialog').remove();
                        }");
                    }
                }
                else
                {
                    // fallback removal if close button not visible
                    await page.EvaluateAsync(
                        @"() => {
                        const modal = document.querySelector('div[data-test=""authModalContainerV2-content""]');
                        if (modal && modal.closest('dialog')) modal.closest('dialog').remove();
                    }");
                }
            }
        }


        private async Task HumanScrollToElementAsync(IPage page, IElementHandle element)
        {
            var box = await element.BoundingBoxAsync();
            if (box == null) return;

            var viewportHeight = page.ViewportSize?.Height ?? 800;
            var startY = Random.Shared.Next(0, viewportHeight / 2);

            var steps = Random.Shared.Next(8, 15);
            for (int i = 0; i <= steps; i++)
            {
                var y = startY + (box.Y - startY) * i / steps + Random.Shared.NextDouble() * 5;
                await page.Mouse.MoveAsync((float)(box.X + box.Width / 2 + Random.Shared.NextDouble() * 3),
                                           (float)y);
                await page.WaitForTimeoutAsync(Random.Shared.Next(50, 150));
            }
        }

        private async Task HumanMoveAndClickAsync(IPage page, IElementHandle element)
        {
            var box = await element.BoundingBoxAsync();
            if (box == null) return;

            var targetX = box.X + box.Width / 2 + Random.Shared.NextDouble() * 3;
            var targetY = box.Y + box.Height / 2 + Random.Shared.NextDouble() * 3;

            // Move cursor in steps
            var steps = Random.Shared.Next(10, 20);
            var startX = Random.Shared.Next(0, (int)page.ViewportSize!.Width / 2);
            var startY = Random.Shared.Next(0, (int)page.ViewportSize.Height / 2);

            for (int i = 0; i <= steps; i++)
            {
                var x = startX + (targetX - startX) * i / steps + Random.Shared.NextDouble();
                var y = startY + (targetY - startY) * i / steps + Random.Shared.NextDouble();
                await page.Mouse.MoveAsync((float)x, (float)y);
                await page.WaitForTimeoutAsync(Random.Shared.Next(20, 70));
            }

            await page.Mouse.ClickAsync((float)targetX, (float)targetY);
        }




    }
}