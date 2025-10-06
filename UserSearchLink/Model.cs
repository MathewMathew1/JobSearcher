using JobSearcher.Account;

namespace JobSearcher.UserSearchLink
{
    public class UserFetchedLink
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserInDatabase User { get; set; } = null!;
        public string Link { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }
}