using JobSearcher.Cv;
using JobSearcher.JobOpening;
using JobSearcher.UserReport;

namespace JobSearcher.Account
{
    public class User
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string ProfilePicture { get; set; }
    }

    public class UserInDatabase : User
    {
        public int Id { get; set; }
        public ICollection<JobOpeningSearcherModel> UserSearches { get; set; } = new List<JobOpeningSearcherModel>();
        public ICollection<UserReportModel> UserReports { get; set; } = new List<UserReportModel>();
        public CvInDatabase? UserCv { get; set; }
    }
}