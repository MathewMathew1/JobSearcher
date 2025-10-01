using Microsoft.Playwright;
using HtmlAgilityPack;

namespace JobSearcher.Job
{
    public class PracujJobSearcher : IJobSearcherService<PracujPlSearchModel>
    {
        private readonly object _lock = new();

        public async Task<List<JobInfo>> GetJobOfferings(PracujPlSearchModel search, SearchedLink searchedLinks, int maxAmount = 10)
        {
            var jobs = new List<JobInfo>();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            
            var url = $"https://www.pracuj.pl/praca/{Uri.EscapeDataString(search.JobSearched)};kw/{Uri.EscapeDataString(search.Location)}";
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            int totalCollected = 0;

            while (totalCollected < maxAmount)
            {
                var result = await Task.WhenAny(
                    page.WaitForSelectorAsync("div.listing_ohw4t83", new PageWaitForSelectorOptions { Timeout = 12000 }),
                    page.WaitForSelectorAsync("div.no-results", new PageWaitForSelectorOptions { Timeout = 12000 })
                );

                var noResult = await page.QuerySelectorAsync("div.no-results");
                if (noResult != null)
                {
                    return jobs;
                }

                var content = await page.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'results__list__item')]");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        if (totalCollected >= maxAmount)
                            break;

                        var linkNode = node.SelectSingleNode(".//a[contains(@class,'job-link')]");
                        var link = linkNode?.GetAttributeValue("href", null);
                        if (link == null) continue;

                        lock (_lock)
                        {
                            if (searchedLinks.SearchedInDatabase.Contains(link) || searchedLinks.NewLinks.Contains(link))
                                continue;

                            searchedLinks.NewLinks.Add(link);
                        }

                        var title = linkNode.InnerText.Trim();
                        var employer = node.SelectSingleNode(".//span[contains(@class,'employer')]")?.InnerText.Trim();
                        var location = node.SelectSingleNode(".//span[contains(@class,'location')]")?.InnerText.Trim();
                        var salary = node.SelectSingleNode(".//span[contains(@class,'salary')]")?.InnerText.Trim();

                        jobs.Add(new JobInfo
                        {
                            Name = title ?? "Unknown",
                            Link = link.StartsWith("http") ? link : "https://www.pracuj.pl" + link,
                            Description = $"{employer} | {location} | {salary}",
                            ImageLink = node.SelectSingleNode(".//img")?.GetAttributeValue("src", null)
                        });

                        totalCollected++;
                    }
                }

                var nextButton = await page.QuerySelectorAsync("a.pagination__next");
                if (nextButton == null) break;

                await nextButton.ClickAsync();
                await page.WaitForTimeoutAsync(2000);
            }

            return jobs;
        }
    }
}
