namespace JobSearch.Sanitizer
{
    public interface ISanitizerService
    {
        string SanitizeHtmlDocument(string htmlDocument);
    }
}