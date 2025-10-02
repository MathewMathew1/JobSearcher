namespace JobSearcher.Cv
{
    public interface IUserCvStorageService
    {
        Task<bool> UploadCvAsync(int userId, string fileNameKey, string fileName);
        Task DeleteCvAsync(string key, int userId);
        Task UpdateFileNameKeyAsync(int userId, string newFileNameKey, string fileName);
    }
}