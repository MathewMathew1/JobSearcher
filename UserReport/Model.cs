using JobSearcher.Account;

namespace JobSearcher.UserReport
{
    public class UserReportModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string DataJson { get; set; } = default!;
        public bool SeenByUser { get; set; } = false;
    }


    public class UserFetchedLink
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public UserInDatabase User { get; set; } = null!;

        public string Link { get; set; } = string.Empty;

        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }

}
