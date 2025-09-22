using Microsoft.EntityFrameworkCore;
using JobSearcher.Models;
using JobSearcher.Account;
using JobSearcher.JobOpening;

namespace JobSearcher.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<UserInDatabase> Users { get; set; }
        public DbSet<JobOpeningSearcherModel> UserSearches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobOpeningSearcherModel>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.HasIndex(e => new { e.UserId, e.Site, e.JobSearched, e.Location })
                    .IsUnique();
            });
        }
    }


}
