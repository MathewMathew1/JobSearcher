using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSearcher.Migrations
{
    /// <inheritdoc />
    public partial class AddingJobSearches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSearches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Site = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    JobSearched = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSearches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSearches_UserId",
                table: "UserSearches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSearches_UserId_Site_JobSearched_Location",
                table: "UserSearches",
                columns: new[] { "UserId", "Site", "JobSearched", "Location" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSearches");
        }
    }
}
