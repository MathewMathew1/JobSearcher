using JobSearcher.Data;
using Microsoft.EntityFrameworkCore;

namespace JobSearcher.Cv
{
    public class UserCvStorage: IUserCvStorageService
    {
        private readonly ILogger<UserCvStorage> _logger;
        private readonly AppDbContext _dbContext;

        public UserCvStorage(ILogger<UserCvStorage> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<bool> UploadCvAsync(int userId, string fileNameKey)
        {
            try
            {
                var newCv = new CvInDatabase
                {
                    UserId = userId,
                    AwsS3Key = fileNameKey,
                    UploadedAt = DateTime.UtcNow
                };

                await _dbContext.UserCvs.AddAsync(newCv);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error uploading CV for user {UserId}", userId);
                return false;
            }

        }

        public async Task DeleteCvAsync(string key, int userId)
        {
            var cv = await _dbContext.UserCvs.FirstOrDefaultAsync(c => c.AwsS3Key == key && c.UserId == userId);
            if (cv != null)
            {
                _dbContext.UserCvs.Remove(cv);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        public async Task UpdateFileNameKeyAsync(int userId, string newFileNameKey)
        {
            var cv = await _dbContext.UserCvs.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cv != null)
            {
                cv.AwsS3Key = newFileNameKey;
                cv.LastUpdated = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}