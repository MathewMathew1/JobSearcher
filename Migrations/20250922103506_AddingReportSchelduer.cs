using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSearcher.Migrations
{
    /// <inheritdoc />
    public partial class AddingReportSchelduer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserReportSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReportSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReportSchedules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    UserReportScheduleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTimes_UserReportSchedules_UserReportScheduleId",
                        column: x => x.UserReportScheduleId,
                        principalTable: "UserReportSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportTimes_UserReportScheduleId_LocalTime",
                table: "ReportTimes",
                columns: new[] { "UserReportScheduleId", "LocalTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReportSchedules_UserId",
                table: "UserReportSchedules",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSearches_Users_UserId",
                table: "UserSearches",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSearches_Users_UserId",
                table: "UserSearches");

            migrationBuilder.DropTable(
                name: "ReportTimes");

            migrationBuilder.DropTable(
                name: "UserReportSchedules");
        }
    }
}
