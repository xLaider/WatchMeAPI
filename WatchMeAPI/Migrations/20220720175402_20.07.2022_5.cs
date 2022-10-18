using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchMeAPI.Migrations
{
    public partial class _20072022_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "Notifications");
        }
    }
}
