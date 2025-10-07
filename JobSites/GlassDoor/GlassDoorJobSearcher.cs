using Microsoft.Playwright;
using HtmlAgilityPack;
using JobSearch.Utils;

namespace JobSearcher.Job
{
    public class GlassDoorJobSearcher : IJobSearcherService<GlassDoorSearchModel>
    {
        private readonly object _lock = new();
        private readonly ILogger<GlassDoorJobSearcher> _logger;

        public GlassDoorJobSearcher(ILogger<GlassDoorJobSearcher> logger)
        {
            _logger = logger;
        }


        public async Task<List<JobInfo>> GetJobOfferings(GlassDoorSearchModel search, SearchedLink searchedLinks, int maxAmount = 10)
        {
            var jobs = new List<JobInfo>();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            var url = $"https://www.glassdoor.com/Job/{search.Location}-{search.JobSearched}-jobs-SRCH_IL.0,6_IN193_KO7,16.htm";
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

                    string extensiveDescription = string.Empty;
                    try
                    {
                        var jobPage = await browser.NewPageAsync();
                        var jobUrl = link.StartsWith("http") ? link : "https://www.glassdoor.com" + link;

                        await jobPage.GotoAsync(jobUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                        var descElement = await jobPage.WaitForSelectorAsync(
                            "div.JobDetails_jobDescription__uW_fK",
                            new PageWaitForSelectorOptions { Timeout = 4000 }
                        );
                        if (descElement != null)
                            extensiveDescription = await descElement.InnerTextAsync();

                        await jobPage.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error retrieving extensive description: {ex.Message}");
                    }

                    jobs.Add(new JobInfo
                    {
                        Name = title,
                        Link = normalizeLink,
                        Description = $"{employer} | {location} | {salary}",
                        ExtensiveDescription = extensiveDescription,
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




    }
}