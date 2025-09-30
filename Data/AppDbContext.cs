using Microsoft.EntityFrameworkCore;
using JobSearcher.Models;
using JobSearcher.Account;
using JobSearcher.JobOpening;
using JobSearcher.Report;
using JobSearcher.UserReport;
using JobSearcher.UserSearchLink;

namespace JobSearcher.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<UserInDatabase> Users { get; set; }
        public DbSet<JobOpeningSearcherModel> UserSearches { get; set; }

        public DbSet<UserReportSchedule> UserReportSchedules { get; set; }
        public DbSet<ReportTime> ReportTimes { get; set; }
        public DbSet<UserReportModel> UserReports { get; set; }

        public DbSet<UserFetchedLink> UserFetchedLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobOpeningSearcherModel>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            
                entity.HasOne<UserInDatabase>()
                      .WithMany(u => u.UserSearches)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.Site, e.JobSearched, e.Location })
                    .IsUnique();
            });

            modelBuilder.Entity<UserReportSchedule>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.Property(e => e.TimeZoneId)
                    .IsRequired();

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<UserReportSchedule>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ReportTimes)
                    .WithOne(rt => rt.UserReportSchedule)
                    .HasForeignKey(rt => rt.UserReportScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReportTime>(entity =>
            {
                entity.Property(rt => rt.LocalTime)
                        .IsRequired();

                entity.HasIndex(rt => new { rt.UserReportScheduleId, rt.LocalTime })
                        .IsUnique();

                entity.HasOne(rt => rt.UserReportSchedule)
                    .WithMany(urs => urs.ReportTimes)
                    .HasForeignKey(rt => rt.UserReportScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserReportModel>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DataJson)
                      .IsRequired();


                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne<UserInDatabase>()
                      .WithMany(u => u.UserReports)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserFetchedLink>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Link)
                        .IsRequired();

                entity.Property(e => e.FetchedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => new { e.UserId, e.Link })
                        .IsUnique();

                entity.HasOne(e => e.User)
                        .WithMany()
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }


}
