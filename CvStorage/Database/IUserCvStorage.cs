namespace JobSearcher.Cv
{
    public interface IUserCvStorageService
    {
        Task<bool> UploadCvAsync(int userId, string fileNameKey);
        Task DeleteCvAsync(string key, int userId);
        Task UpdateFileNameKeyAsync(int userId, string newFileNameKey);
    }
}