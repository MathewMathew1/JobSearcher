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
            Console.WriteLine($"Searching for {search.JobSearched} in {search.Location} on Pracuj.pl");

            var url = $"https://www.pracuj.pl/praca/{Uri.EscapeDataString(search.JobSearched)};kw/{Uri.EscapeDataString(search.Location)};wp?rd=30";
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            int totalCollected = 0;

            while (totalCollected < maxAmount)
            {
                await page.WaitForSelectorAsync("div[data-test='section-offers']", new PageWaitForSelectorOptions { Timeout = 12000 });

                var content = await page.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var nodes = doc.DocumentNode.SelectNodes("//div[@data-test='section-offers']//div[contains(@data-test,'offer')]");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        if (totalCollected >= maxAmount) break;

                        var linkNode = node.SelectSingleNode(".//a[@data-test='link-offer-title']");
                        var link = linkNode?.GetAttributeValue("href", null);
                        if (link == null) continue;

                        lock (_lock)
                        {
                            if (searchedLinks.SearchedInDatabase.Contains(link) || searchedLinks.NewLinks.Contains(link))
                                continue;

                            searchedLinks.NewLinks.Add(link);
                        }

                        var title = linkNode.InnerText.Trim();
                        var employer = node.SelectSingleNode(".//h3[@data-test='text-company-name']")?.InnerText.Trim();
                        var location = node.SelectSingleNode(".//h4[@data-test='text-region']")?.InnerText.Trim();

                        var salaryNode = node.SelectSingleNode(".//ul[contains(@class,'tiles_bfrsaoj')]/li[contains(text(),'zł')]");
                        var salary = salaryNode?.InnerText.Trim();

                        var img = node.SelectSingleNode(".//img[@data-test='image-responsive']")?.GetAttributeValue("src", null);

                        jobs.Add(new JobInfo
                        {
                            Name = title ?? "Unknown",
                            Link = link.StartsWith("http") ? link : "https://www.pracuj.pl" + link,
                            Description = $"{employer} | {location} | {salary}",
                            ImageLink = img
                        });

                        totalCollected++;
                    }
                }

                var nextButton = await page.QuerySelectorAsync("button[data-test='pagination-next']");
                if (nextButton == null) break;

                await nextButton.ClickAsync();
                await page.WaitForTimeoutAsync(2000);
            }

            return jobs;
        }
    }

}
