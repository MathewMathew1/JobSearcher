using Microsoft.Playwright;
using HtmlAgilityPack;

namespace JobSearcher.Job
{
    public class GlassDoorJobSearcher : IGlassDoorJobSearcher
    {
        public async Task<List<JobInfo>> GetJobOfferings(GlassDoorJobSearch search, int maxAmount = 10)
        {
            var jobs = new List<JobInfo>();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();

            var url = $"https://www.glassdoor.com/Job/{search.Location}-{search.JobSearched}-jobs-SRCH_IL.0,6_IN193_KO7,23.htm";

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            await page.WaitForSelectorAsync("li[data-test='jobListing']", new PageWaitForSelectorOptions { Timeout = 12000 });

            var content = await page.ContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            var nodes = doc.DocumentNode.SelectNodes("//li[@data-test='jobListing']");
            if (nodes != null)
            {
                foreach (var node in nodes.Take(maxAmount))
                {
                    var titleNode = node.SelectSingleNode(".//a[contains(@class,'JobCard_jobTitle')]");
                    var link = titleNode?.GetAttributeValue("href", "");
                    var title = titleNode?.InnerText.Trim();
                    var employer = node.SelectSingleNode(".//span[contains(@class,'EmployerProfile_compactEmployerName')]")?.InnerText.Trim();
                    var location = node.SelectSingleNode(".//div[@data-test='emp-location']")?.InnerText.Trim();
                    var salary = node.SelectSingleNode(".//div[@data-test='detailSalary']")?.InnerText.Trim();
                    var img = node.SelectSingleNode(".//img[contains(@class,'avatar-base_Image')]")?.GetAttributeValue("src", null);

                    jobs.Add(new JobInfo
                    {
                        Name = title ?? "Unknown",
                        Link = link?.StartsWith("http") == true ? link : "https://www.glassdoor.com" + link,
                        Description = $"{employer} | {location} | {salary}",
                        ImageLink = img
                    });
                }
            }


            return jobs;
        }


    }
}