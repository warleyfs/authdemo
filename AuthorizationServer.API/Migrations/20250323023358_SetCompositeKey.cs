using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthorizationServer.API.Migrations
{
    /// <inheritdoc />
    public partial class SetCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions",
                columns: new[] { "Id", "UserId", "KeyId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions",
                columns: new[] { "UserId", "KeyId", "ClientId" });
        }
    }
}
