using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSearcher.Migrations
{
    /// <inheritdoc />
    public partial class Addedfilenametofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Filename",
                table: "UserCvs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Filename",
                table: "UserCvs");
        }
    }
}
