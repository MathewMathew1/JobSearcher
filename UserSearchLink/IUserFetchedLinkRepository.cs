namespace JobSearcher.UserSearchLink
{
    public interface IUserFetchedLinkRepository
    {
        Task<List<string>> GetAllLinksAsync(int userId);
        Task SaveLinksAsync(int userId, IEnumerable<string> links);
    }
}