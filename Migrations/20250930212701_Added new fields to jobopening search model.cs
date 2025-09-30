using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSearcher.Migrations
{
    /// <inheritdoc />
    public partial class Addednewfieldstojobopeningsearchmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "UserSearches",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sort",
                table: "UserSearches",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Start",
                table: "UserSearches",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "UserSearches");

            migrationBuilder.DropColumn(
                name: "Sort",
                table: "UserSearches");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "UserSearches");
        }
    }
}
