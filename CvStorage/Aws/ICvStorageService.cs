
namespace JobSearcher.Cv
{
    public interface ICvStorageService
    {
        Task<string> UploadCvAsync(Stream fileStream, string fileName, string contentType);
        Task DeleteCvAsync(string key);
        Task<Stream> DownloadCvAsync(string key);
    }
}