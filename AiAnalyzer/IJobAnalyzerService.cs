namespace JobSearcher.AiAnalyzer
{
    public interface IJobAnalyzerService
    {
        Task<float> AnalyzeCvAsync(string cvText, string jobDescription);
    }
}
