using JobSearcher.Account;

namespace JobSearcher.Cv
{

    public class CvInDatabase
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required string AwsS3Key { get; set; }
        public required DateTime UploadedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public UserInDatabase User { get; set; }
    }
}