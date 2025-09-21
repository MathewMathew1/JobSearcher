namespace JobSearcher.Account
{
    public interface IAccount
    {
        Task<UserInDatabase?> GetUser(string email);
        Task<UserInDatabase> SetUser(User user);
        Task<UserInDatabase?> GetUserById(int id);
    }
}