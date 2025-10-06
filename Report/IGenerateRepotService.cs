namespace JobSearcher.Report
{
    public interface IGenerateReportService
    {
        Task GenerateReportForUser(int userId, bool analyzeMatch);
    }
}