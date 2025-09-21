using Microsoft.EntityFrameworkCore;
using JobSearcher.Models;
using JobSearcher.Account;

namespace JobSearcher.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<UserInDatabase> Users { get; set; }

    }
}
