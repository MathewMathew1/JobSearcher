using Microsoft.Playwright;
using HtmlAgilityPack;

namespace JobSearcher.Job
{
    public class IndeedJobSearcher : IJobSearcherService
    {
        private readonly object _lock = new();

        public async Task<List<JobInfo>> GetJobOfferings(JobSearchModel search, SearchedLink searchedLinks, int maxAmount = 10)
        {
            var jobs = new List<JobInfo>();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            var query = Uri.EscapeDataString(search.JobSearched);
            var location = Uri.EscapeDataString(search.Location);
            var url = $"https://www.indeed.com/jobs?q={query}&l={location}";

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            int totalCollected = 0;

            while (totalCollected < maxAmount)
            {
                await page.WaitForSelectorAsync("div.job_seen_beacon", new PageWaitForSelectorOptions { Timeout = 12000 });
                var content = await page.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'job_seen_beacon')]");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        if (totalCollected >= maxAmount)
                            break;

                        var titleNode = node.SelectSingleNode(".//h2[contains(@class,'jobTitle')]/span");
                        var linkNode = node.SelectSingleNode(".//a[contains(@class,'tapItem')]");
                        var link = linkNode?.GetAttributeValue("href", null);

                        lock (_lock)
                        {
                            if (link == null || searchedLinks.SearchedInDatabase.Contains(link) || searchedLinks.NewLinks.Contains(link))
                            {
                                continue;
                            }

                            searchedLinks.NewLinks.Add(link);
                        }

                        var title = titleNode?.InnerText.Trim();
                        var employer = node.SelectSingleNode(".//span[@class='companyName']")?.InnerText.Trim();
                        var locationText = node.SelectSingleNode(".//div[contains(@class,'companyLocation')]")?.InnerText.Trim();
                        var salary = node.SelectSingleNode(".//span[@class='salary-snippet']")?.InnerText.Trim();

                        jobs.Add(new JobInfo
                        {
                            Name = title ?? "Unknown",
                            Link = link?.StartsWith("http") == true ? link : "https://www.indeed.com" + link,
                            Description = $"{employer} | {locationText} | {salary}",
                            ImageLink = null
                        });

                        totalCollected++;
                    }
                }

                var nextButton = await page.QuerySelectorAsync("a[aria-label='Next']");
                if (nextButton == null || totalCollected >= maxAmount)
                {
                    break;
                }

                await nextButton.ClickAsync();
                await page.WaitForTimeoutAsync(2000);
            }

            return jobs;
        }
    }
}
