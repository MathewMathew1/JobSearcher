using Amazon.S3;
using Amazon.S3.Model;


namespace JobSearcher.Cv
{
    public class CvStorageService : ICvStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public CvStorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3BucketName"];
        }

        public async Task<string> UploadCvAsync(Stream fileStream, string fileName, string contentType)
        {
            var key = $"cvs/{Guid.NewGuid()}_{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);
            return key;
        }

        public async Task DeleteCvAsync(string key)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}
