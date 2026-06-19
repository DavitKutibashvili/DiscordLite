using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordLite_API.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteCodeToServe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Servers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InviteExpiresAt",
                table: "Servers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "InviteExpiresAt",
                table: "Servers");
        }
    }
}
