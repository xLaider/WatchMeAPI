using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchMeAPI.Migrations
{
    public partial class _20072022_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friend_AspNetUsers_ApplicationUserId",
                table: "Friend");

            migrationBuilder.DropIndex(
                name: "IX_Friend_ApplicationUserId",
                table: "Friend");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Friend");

            migrationBuilder.CreateTable(
                name: "ApplicationUserFriend",
                columns: table => new
                {
                    FriendsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserFriend", x => new { x.FriendsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserFriend_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserFriend_Friend_FriendsId",
                        column: x => x.FriendsId,
                        principalTable: "Friend",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFriend_UsersId",
                table: "ApplicationUserFriend",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserFriend");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Friend",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friend_ApplicationUserId",
                table: "Friend",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friend_AspNetUsers_ApplicationUserId",
                table: "Friend",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
