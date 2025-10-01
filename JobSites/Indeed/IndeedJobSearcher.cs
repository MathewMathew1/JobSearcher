using Microsoft.Playwright;
using HtmlAgilityPack;

namespace JobSearcher.Job
{
    public class IndeedJobSearcher : IJobSearcherService<IndeedSearchModel>
    {
        private readonly object _lock = new();

        public async Task<List<JobInfo>> GetJobOfferings(IndeedSearchModel search, SearchedLink searchedLinks, int maxAmount = 10)
        {
            var jobs = new List<JobInfo>();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            string url = $"https://{search.CountryCode}.indeed.com/jobs?q={Uri.EscapeDataString(search.JobSearched)}&l={Uri.EscapeDataString(search.Location)}";

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            int totalCollected = 0;

            while (totalCollected < maxAmount)
            {
                // Get all cards currently rendered
                var jobCards = await page.QuerySelectorAllAsync("div.job_seen_beacon");
                if (jobCards.Count == 0)
                {
                    Console.WriteLine("No job results found.");
                    break;
                }

                foreach (var card in jobCards)
                {
                    if (totalCollected >= maxAmount)
                        break;

                    // Scroll and click card to open description
                    await card.ScrollIntoViewIfNeededAsync();
                    await card.ClickAsync();

                    // Wait for description to appear / update
                    var descElement = await page.WaitForSelectorAsync(
                        "div#jobDescriptionText",
                        new PageWaitForSelectorOptions { Timeout = 10000 }
                    );

                    string description = string.Empty;
                    if (descElement != null)
                        description = await descElement.InnerTextAsync();

                    // Extract basic info from card
                    var title = await card.QuerySelectorAsync("h2.jobTitle span") is IElementHandle t
                        ? await t.InnerTextAsync()
                        : "Unknown";

                    var link = await card.QuerySelectorAsync("a.jcs-JobTitle") is IElementHandle a
                        ? await a.GetAttributeAsync("href")
                        : null;

                    if (!string.IsNullOrEmpty(link) && !link.StartsWith("http"))
                        link = "https://www.indeed.com" + link;

                    if (string.IsNullOrEmpty(link))
                        continue;

                    lock (_lock)
                    {
                        if (searchedLinks.SearchedInDatabase.Contains(link) || searchedLinks.NewLinks.Contains(link))
                            continue;
                        searchedLinks.NewLinks.Add(link);
                    }

                    var employer = await card.QuerySelectorAsync("span[data-testid='company-name']") is IElementHandle e
                        ? await e.InnerTextAsync()
                        : null;

                    var locationText = await card.QuerySelectorAsync("div[data-testid='text-location']") is IElementHandle l
                        ? await l.InnerTextAsync()
                        : null;

                    var salary = await card.QuerySelectorAsync("span.salary-snippet") is IElementHandle s
                        ? await s.InnerTextAsync()
                        : null;

                    jobs.Add(new JobInfo
                    {
                        Name = title,
                        Link = link,
                        Description = $"{employer} | {locationText} | {salary}",
                        ExtensiveDescription = description,
                        ImageLink = null
                    });

                    totalCollected++;
                }

                // Go to next page if needed
                if (totalCollected >= maxAmount)
                    break;

                var nextButton = await page.QuerySelectorAsync("a[aria-label='Next']");
                if (nextButton == null) break;

                await nextButton.ClickAsync();
                await page.WaitForTimeoutAsync(2000);
            }

            await browser.CloseAsync();
            Console.WriteLine($"Total jobs collected: {jobs.Count}");
            return jobs;
        }

    }
}
