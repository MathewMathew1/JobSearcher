namespace JobSearcher.AiAnalyzer
{
    public interface IJobAnalyzerService
    {
        Task<float> AnalyzeCvAsync(string cvText, string jobDescription);
        Task<float[]> AnalyzeJobDescriptionsAsync(string cvText, List<string> jobDescriptions);
    }
}
