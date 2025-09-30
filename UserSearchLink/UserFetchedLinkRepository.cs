using Microsoft.EntityFrameworkCore;
using JobSearcher.Data;
using JobSearcher.UserSearchLink;

namespace JobSearcher.Job
{
    public class UserFetchedLinkRepository : IUserFetchedLinkRepository
    {
        private readonly AppDbContext _context;

        public UserFetchedLinkRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetAllLinksAsync(int userId)
        {
            var cutoff = DateTime.UtcNow.AddMonths(-2);
            return await _context.UserFetchedLinks
                                 .Where(l => l.UserId == userId && l.FetchedAt >= cutoff)
                                 .Select(l => l.Link)
                                 .ToListAsync();
        }

        public async Task SaveLinksAsync(int userId, IEnumerable<string> links)
        {
            var cutoff = DateTime.UtcNow.AddMonths(-2);

            var existingLinks = await _context.UserFetchedLinks
                                              .Where(l => l.UserId == userId && l.FetchedAt >= cutoff)
                                              .Select(l => l.Link)
                                              .ToListAsync();

            var newLinks = links.Except(existingLinks).Select(link => new UserFetchedLink
            {
                UserId = userId,
                Link = link,
                FetchedAt = DateTime.UtcNow
            });

            await _context.UserFetchedLinks.AddRangeAsync(newLinks);
            await _context.SaveChangesAsync();
        }
    }
}
