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
    }
}