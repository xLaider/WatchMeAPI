using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchMeAPI.Migrations
{
    public partial class _20072022_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Friend",
                table: "Friend");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Friend",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Friend",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friend",
                table: "Friend",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Friend",
                table: "Friend");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Friend");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Friend",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friend",
                table: "Friend",
                column: "Name");
        }
    }
}
