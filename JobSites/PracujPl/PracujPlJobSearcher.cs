using Microsoft.Playwright;
using HtmlAgilityPack;
using JobSearch.Utils;

namespace JobSearcher.Job
{
    public class PracujJobSearcher : IJobSearcherService<PracujPlSearchModel>
    {
        private readonly object _lock = new();

        private readonly ILogger<PracujJobSearcher> _logger;

        public PracujJobSearcher(ILogger<PracujJobSearcher> logger)
        {
            _logger = logger;
        }

        public async Task<List<JobInfo>> GetJobOfferings(PracujPlSearchModel search, SearchedLink searchedLinks, int maxAmount = 10)
        {
            var jobs = new List<JobInfo>();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            var page = await browser.NewPageAsync();
            Console.WriteLine($"Searching for {search.JobSearched} in {search.Location} on Pracuj.pl");

            var url = $"https://www.pracuj.pl/praca/{Uri.EscapeDataString(search.JobSearched)};kw/{Uri.EscapeDataString(search.Location)};wp?rd=30";
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            int totalCollected = 0;

            while (totalCollected < maxAmount)
            {
                // Wait for job listings container
                await page.WaitForSelectorAsync("div[data-test='section-offers']", new PageWaitForSelectorOptions { Timeout = 20000 });

                var content = await page.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var nodes = doc.DocumentNode.SelectNodes("//div[@data-test='section-offers']//div[contains(@data-test,'offer')]");
                if (nodes == null || nodes.Count == 0) break;

                foreach (var node in nodes)
                {
                    if (totalCollected >= maxAmount) break;

                    var linkNode = node.SelectSingleNode(".//a[@data-test='link-offer-title']");
                    var link = linkNode?.GetAttributeValue("href", null);
                    if (link == null) continue;

                    link = link.StartsWith("http") ? link : "https://www.pracuj.pl" + link;
                    var normalizeLink = LinkHelper.NormalizeLink(link);

                    lock (_lock)
                    {
                        if (searchedLinks.SearchedInDatabase.Contains(normalizeLink) || searchedLinks.NewLinks.Contains(normalizeLink))
                            continue;

                        searchedLinks.NewLinks.Add(normalizeLink);
                    }

                    var title = linkNode.InnerText.Trim();
                    var employer = node.SelectSingleNode(".//h3[@data-test='text-company-name']")?.InnerText.Trim();
                    var location = node.SelectSingleNode(".//h4[@data-test='text-region']")?.InnerText.Trim();
                    var salaryNode = node.SelectSingleNode(".//ul[contains(@class,'tiles_bfrsaoj')]/li[contains(text(),'zł')]");
                    var salary = salaryNode?.InnerText.Trim();
                    var img = node.SelectSingleNode(".//img[@data-test='image-responsive']")?.GetAttributeValue("src", null);

                    string requirementsText = string.Empty;
                    try
                    {
                        // Reuse the same page for job details
                        await page.GotoAsync(link, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

                        var requirementsElement = await page.QuerySelectorAsync("section[data-test='section-requirements']");
                        if (requirementsElement != null)
                            requirementsText = await requirementsElement.InnerTextAsync();

                        // Go back to the main search page
                        await page.GoBackAsync();
                        await page.WaitForTimeoutAsync(500); // small delay to stabilize
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error retrieving requirements for {link}: {ex.Message}");
                    }

                    jobs.Add(new JobInfo
                    {
                        Name = title ?? "Unknown",
                        Link = normalizeLink,
                        Description = $"{employer} | {location} | {salary}",
                        ImageLink = img,
                        ExtensiveDescription = requirementsText
                    });

                    totalCollected++;
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
