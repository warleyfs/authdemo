using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthorizationServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    KeyId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => new { x.UserId, x.KeyId, x.ClientId });
                    table.ForeignKey(
                        name: "FK_UserSessions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSessions_SigningKeys_KeyId",
                        column: x => x.KeyId,
                        principalTable: "SigningKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ClientId",
                table: "UserSessions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_KeyId",
                table: "UserSessions",
                column: "KeyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions");
        }
    }
}
