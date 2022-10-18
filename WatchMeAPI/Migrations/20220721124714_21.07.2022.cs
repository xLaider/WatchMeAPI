using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchMeAPI.Migrations
{
    public partial class _21072022 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Creator",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Creator",
                table: "Playlists");
        }
    }
}
