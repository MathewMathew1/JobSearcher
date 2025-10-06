namespace JobSearcher.Cv
{
    public interface ICvParserService
    {
        string ExtractText(Stream fileStream, string fileName);
    }
}
