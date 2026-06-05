using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordLite_API.Migrations
{
    /// <inheritdoc />
    public partial class Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessageChat_AspNetUsers_User1Id",
                table: "DirectMessageChat");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessageChat_AspNetUsers_User2Id",
                table: "DirectMessageChat");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_AspNetUsers_ReceivedById",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_AspNetUsers_RequestedById",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_DirectMessageChat_ChatId",
                table: "Message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Friendship",
                table: "Friendship");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DirectMessageChat",
                table: "DirectMessageChat");

            migrationBuilder.RenameTable(
                name: "Friendship",
                newName: "Friendships");

            migrationBuilder.RenameTable(
                name: "DirectMessageChat",
                newName: "DirectMessageChats");

            migrationBuilder.RenameIndex(
                name: "IX_Friendship_RequestedById_ReceivedById",
                table: "Friendships",
                newName: "IX_Friendships_RequestedById_ReceivedById");

            migrationBuilder.RenameIndex(
                name: "IX_Friendship_ReceivedById",
                table: "Friendships",
                newName: "IX_Friendships_ReceivedById");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessageChat_User2Id",
                table: "DirectMessageChats",
                newName: "IX_DirectMessageChats_User2Id");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessageChat_User1Id_User2Id",
                table: "DirectMessageChats",
                newName: "IX_DirectMessageChats_User1Id_User2Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friendships",
                table: "Friendships",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DirectMessageChats",
                table: "DirectMessageChats",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessageChats_AspNetUsers_User1Id",
                table: "DirectMessageChats",
                column: "User1Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessageChats_AspNetUsers_User2Id",
                table: "DirectMessageChats",
                column: "User2Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_ReceivedById",
                table: "Friendships",
                column: "ReceivedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_RequestedById",
                table: "Friendships",
                column: "RequestedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_DirectMessageChats_ChatId",
                table: "Message",
                column: "ChatId",
                principalTable: "DirectMessageChats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessageChats_AspNetUsers_User1Id",
                table: "DirectMessageChats");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessageChats_AspNetUsers_User2Id",
                table: "DirectMessageChats");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_ReceivedById",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_RequestedById",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_DirectMessageChats_ChatId",
                table: "Message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Friendships",
                table: "Friendships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DirectMessageChats",
                table: "DirectMessageChats");

            migrationBuilder.RenameTable(
                name: "Friendships",
                newName: "Friendship");

            migrationBuilder.RenameTable(
                name: "DirectMessageChats",
                newName: "DirectMessageChat");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_RequestedById_ReceivedById",
                table: "Friendship",
                newName: "IX_Friendship_RequestedById_ReceivedById");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_ReceivedById",
                table: "Friendship",
                newName: "IX_Friendship_ReceivedById");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessageChats_User2Id",
                table: "DirectMessageChat",
                newName: "IX_DirectMessageChat_User2Id");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessageChats_User1Id_User2Id",
                table: "DirectMessageChat",
                newName: "IX_DirectMessageChat_User1Id_User2Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friendship",
                table: "Friendship",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DirectMessageChat",
                table: "DirectMessageChat",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessageChat_AspNetUsers_User1Id",
                table: "DirectMessageChat",
                column: "User1Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessageChat_AspNetUsers_User2Id",
                table: "DirectMessageChat",
                column: "User2Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_AspNetUsers_ReceivedById",
                table: "Friendship",
                column: "ReceivedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_AspNetUsers_RequestedById",
                table: "Friendship",
                column: "RequestedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_DirectMessageChat_ChatId",
                table: "Message",
                column: "ChatId",
                principalTable: "DirectMessageChat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
