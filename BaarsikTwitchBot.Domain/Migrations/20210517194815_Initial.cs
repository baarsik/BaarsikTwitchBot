using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaarsikTwitchBot.Domain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SongInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VideoId = table.Column<string>(type: "TEXT", nullable: true),
                    Limitation = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Login = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    IsFollower = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBanned = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBannedSongPlayer = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpitsReceived = table.Column<uint>(type: "INTEGER", nullable: true),
                    LicksReceived = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongInfo");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
