using JobSearcher.Data;
using Microsoft.EntityFrameworkCore;

namespace JobSearcher.JobOpening
{
    public class JobOpeningSearcher : IJobOpeningSearcher
    {
        private readonly ILogger<JobOpeningSearcher> _logger;
        private AppDbContext _database;

        public JobOpeningSearcher(ILogger<JobOpeningSearcher> logger, AppDbContext database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task<JobOpeningSearcherModel> CreateJobOpeningSearch(SearchInfo searchInfo, int UserId)
        {
            JobOpeningSearcherModel searchInfoInDatabase = new JobOpeningSearcherModel
            {
                UserId = UserId,
                Site = searchInfo.Site,
                Location = searchInfo.Location,
                JobSearched = searchInfo.JobSearched,
                CountryCode = searchInfo.CountryCode,
                IsActive = true
            };

            await _database.UserSearches.AddAsync(searchInfoInDatabase);
            await _database.SaveChangesAsync();

            return searchInfoInDatabase;
        }

        public async Task<JobOpeningSearcherModel> UpdateJobOpeningSearch(SearchInfo searchInfo, int id, int UserId)
        {
            JobOpeningSearcherModel? searchInfoInDatabase = _database.UserSearches.FirstOrDefault(uS => uS.Id == id && uS.UserId == UserId);
            if (searchInfoInDatabase == null)
            {
                return null;
            }

            searchInfoInDatabase.Site = searchInfo.Site;
            searchInfoInDatabase.Location = searchInfo.Location;
            searchInfoInDatabase.JobSearched = searchInfo.JobSearched;
            searchInfoInDatabase.CountryCode = searchInfo.CountryCode;

            await _database.SaveChangesAsync();

            return searchInfoInDatabase;
        }

        public async Task<List<JobOpeningSearcherModel>> GetSearchesByUser(int UserId)
        {
            return await _database.UserSearches
                .Where(s => s.UserId == UserId)
                .ToListAsync();
        }

        public async Task<List<JobOpeningSearcherModel>> GetSearchesByUserAndSite(int UserId, Site site)
        {
            return await _database.UserSearches
                .Where(s => s.UserId == UserId && s.Site == site)
                .ToListAsync();
        }

        public async Task DeleteSearchById(int id, int UserId)
        {
            var search = await _database.UserSearches
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);

            if (search != null)
            {
                _database.UserSearches.Remove(search);
                await _database.SaveChangesAsync();
            }
        }
    }
}