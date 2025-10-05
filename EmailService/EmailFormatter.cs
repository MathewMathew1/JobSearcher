using System.Text;
using JobSearcher.JobOpening;
using JobSearcher.Job;

namespace JobSearch.Emails
{
    public class HtmlEmailReportFormatter : IEmailReportFormatter
    {
        public string FormatReport(IDictionary<Site, List<JobInfo>> resultsBySite, int userId)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'><title>Job Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; color: #333; background: #fafafa; margin: 0; padding: 20px; }");
            sb.AppendLine(".header { background: #004080; color: white; padding: 12px 20px; font-size: 20px; border-radius: 6px; }");
            sb.AppendLine(".site-section { margin-top: 25px; }");
            sb.AppendLine(".site-title { font-size: 18px; font-weight: bold; color: #004080; margin-bottom: 10px; }");
            sb.AppendLine(".job-card { background: white; border: 1px solid #ddd; border-radius: 6px; padding: 10px 15px; margin-bottom: 10px; }");
            sb.AppendLine(".job-title { font-size: 16px; font-weight: bold; margin-bottom: 4px; }");
            sb.AppendLine(".job-link a { color: #1a73e8; text-decoration: none; }");
            sb.AppendLine(".job-link a:hover { text-decoration: underline; }");
            sb.AppendLine(".job-desc { font-size: 14px; color: #444; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine($"<div class='header'>Job Report for User #{userId}</div>");

            foreach (var siteEntry in resultsBySite)
            {
                sb.AppendLine("<div class='site-section'>");
                sb.AppendLine($"<div class='site-title'>{siteEntry.Key}</div>");

                if (siteEntry.Value.Count == 0)
                {
                    sb.AppendLine("<div>No job results found for this site.</div>");
                    sb.AppendLine("</div>");
                    continue;
                }

                foreach (var job in siteEntry.Value)
                {
                    sb.AppendLine("<div class='job-card'>");
                    sb.AppendLine($"<div class='job-title'>{Escape(job.Name)}</div>");
                    sb.AppendLine($"<div class='job-link'><a href='{job.Link}' target='_blank'>View job</a></div>");
                    if (!string.IsNullOrWhiteSpace(job.Description))
                        sb.AppendLine($"<div class='job-desc'>{Escape(Truncate(job.Description, 250))}</div>");
                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string Escape(string text)
        {
            return System.Net.WebUtility.HtmlEncode(text);
        }

        private static string Truncate(string text, int length)
        {
            if (text.Length <= length) return text;
            return text.Substring(0, length) + "...";
        }
    }
}
