using Ganss.Xss;

namespace JobSearch.Sanitizer
{
    public class SanitizerService : ISanitizerService
    {
        private readonly ILogger<SanitizerService> _logger;
        private readonly HtmlSanitizer _htmlSanitizer;

        public SanitizerService(ILogger<SanitizerService> logger)
        {
            _logger = logger;
            _htmlSanitizer = new HtmlSanitizer();
            _htmlSanitizer.AllowedSchemes.Add("data");
            _htmlSanitizer.AllowedAttributes.Add("class");
            _htmlSanitizer.AllowedAttributes.Add("style");
        }

        public string SanitizeHtmlDocument(string htmlDocument)
        {
            return _htmlSanitizer.Sanitize(htmlDocument);
        }
    }
}