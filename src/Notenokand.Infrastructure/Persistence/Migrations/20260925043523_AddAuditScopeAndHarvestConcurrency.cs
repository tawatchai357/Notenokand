using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditScopeAndHarvestConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "HarvestRounds",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AccountId_OccurredAt",
                table: "AuditLogs",
                columns: new[] { "AccountId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_AccountId_OccurredAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "HarvestRounds");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "AuditLogs");
        }
    }
}
