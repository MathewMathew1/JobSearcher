using Microsoft.Playwright;
using HtmlAgilityPack;

namespace JobSearcher.Job
{
    public class IndeedJobSearcher : IJobSearcherService<IndeedSearchModel>
    {
        private readonly object _lock = new();

        public async Task<List<JobInfo>> GetJobOfferings(IndeedSearchModel search, SearchedLink searchedLinks, int maxAmount = 20)
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
            string url = $"https://{search.CountryCode}/jobs?q={Uri.EscapeDataString(search.JobSearched)}&l={Uri.EscapeDataString(search.Location)}";

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            int totalCollected = 0;

            while (totalCollected < maxAmount)
            {
                await page.WaitForSelectorAsync("div.job_seen_beacon", new PageWaitForSelectorOptions { Timeout = 12000 });

                var content = await page.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'job_seen_beacon')]");
                Console.WriteLine($"Found {nodes?.Count ?? 0} job nodes on the page.");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        if (totalCollected >= maxAmount) break;

                        var titleNode = node.SelectSingleNode(".//h2[contains(@class,'jobTitle')]//span");
                        Console.WriteLine($"Processing job node: {titleNode?.InnerText.Trim() ?? "No title found"}");
                        var linkNode = node.SelectSingleNode(".//a[contains(@class,'jcs-JobTitle')]");
                        var link = linkNode?.GetAttributeValue("href", null);

                        Console.WriteLine($"extracted link {link}");

                        if (link == null) continue;

                        if (!link.StartsWith("http"))
                        {
                            link = "https://www.indeed.com" + link;
                        }

                        lock (_lock)
                        {
                            if (searchedLinks.SearchedInDatabase.Contains(link) || searchedLinks.NewLinks.Contains(link))
                                continue;

                            searchedLinks.NewLinks.Add(link);
                        }

                        var title = titleNode?.InnerText.Trim();
                        var employer = node.SelectSingleNode(".//span[@data-testid='company-name']")?.InnerText.Trim();
                        var locationText = node.SelectSingleNode(".//div[@data-testid='text-location']")?.InnerText.Trim();
                        var salary = node.SelectSingleNode(".//span[contains(@class,'salary-snippet')]")?.InnerText.Trim();

                        jobs.Add(new JobInfo
                        {
                            Name = title ?? "Unknown",
                            Link = link,
                            Description = $"{employer} | {locationText} | {salary}",
                            ImageLink = null
                        });

                        totalCollected++;
                    }
                }

                var nextButton = await page.QuerySelectorAsync("a[aria-label='Next']");
                if (nextButton == null || totalCollected >= maxAmount) break;

                await nextButton.ClickAsync();
                await page.WaitForTimeoutAsync(2000);
            }
            await browser.CloseAsync();
            Console.WriteLine($"Total jobs collected: {jobs.Count}");
            return jobs;
        }
    }
}
