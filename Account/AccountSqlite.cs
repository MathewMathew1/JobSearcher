using JobSearcher.Cv;
using JobSearcher.Data;
using Microsoft.EntityFrameworkCore;

namespace JobSearcher.Account
{
    public class AccountMySql : IAccount
    {
        private AppDbContext _database;

        public AccountMySql(AppDbContext database)
        {
            _database = database;
        }

        public async Task<UserInDatabase?> GetUser(string email)
        {
            return await _database.Users.Include(u => u.UserSearches).FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<UserInDatabase?> GetUserById(int id)
        {
            return await _database.Users
            .Include(u => u.UserReports)
            .Include(u => u.UserSearches)
            .Include(u => u.UserCv)
            .AsSplitQuery()
            .FirstOrDefaultAsync(user => user.Id == id);
        }
        
        public async Task<(string Email, CvInDatabase? UserCv)?> GetEmailAndCvByUserId(int userId)
        {
            return await _database.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Email,
                    u.UserCv
                })
                .FirstOrDefaultAsync() is { } result
                ? (result.Email, result.UserCv)
                : null;
        }



        public async Task<UserInDatabase> SetUser(User user)
        {
            var userInDb = new UserInDatabase
            {
                Name = user.Name,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture
            };

            await _database.Users.AddAsync(userInDb);
            await _database.SaveChangesAsync();
            return userInDb;
        }

    }
}