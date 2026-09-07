using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AccountUsers_UserId",
                table: "AccountUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountInvitations_Accounts_AccountId",
                table: "AccountInvitations",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountUsers_AspNetUsers_UserId",
                table: "AccountUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerificationLogs_AspNetUsers_UserId",
                table: "EmailVerificationLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConsents_AspNetUsers_UserId",
                table: "UserConsents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountInvitations_Accounts_AccountId",
                table: "AccountInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountUsers_AspNetUsers_UserId",
                table: "AccountUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerificationLogs_AspNetUsers_UserId",
                table: "EmailVerificationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserConsents_AspNetUsers_UserId",
                table: "UserConsents");

            migrationBuilder.DropIndex(
                name: "IX_AccountUsers_UserId",
                table: "AccountUsers");
        }
    }
}
