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
                    Id = table.Column<Guid>(nullable: false),
                    VideoId = table.Column<string>(nullable: true),
                    Limitation = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Login = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    IsFollower = table.Column<bool>(nullable: false),
                    IsBannedSongPlayer = table.Column<bool>(nullable: false),
                    SpitsReceived = table.Column<long>(nullable: true),
                    LicksReceived = table.Column<long>(nullable: true)
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
