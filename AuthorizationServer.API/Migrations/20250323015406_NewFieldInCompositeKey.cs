using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthorizationServer.API.Migrations
{
    /// <inheritdoc />
    public partial class NewFieldInCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Unique_Expires",
                table: "UserSessions",
                column: "Expires");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Unique_Expires",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserSessions");
        }
    }
}
