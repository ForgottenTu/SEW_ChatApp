using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Model.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdNullableOnChatRoomMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomMembers_AspNetUsers_UserId",
                table: "ChatRoomMembers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ChatRoomMembers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "ChatRoomMembers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomMembers_AspNetUsers_UserId",
                table: "ChatRoomMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomMembers_AspNetUsers_UserId",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "ChatRoomMembers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ChatRoomMembers",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomMembers_AspNetUsers_UserId",
                table: "ChatRoomMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
