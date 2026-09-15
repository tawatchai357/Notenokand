using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountFinanceAndReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "FinancialTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FinancialTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "FinancialTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Counterparty",
                table: "FinancialTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "FinancialTransactions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "FinancialTransactions",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExpenseCategories",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "ExpenseCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "ExpenseCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ExpenseCategories",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateTable(
                name: "TransactionReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "char(64)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionReceipts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionReceipts_FinancialTransactions_FinancialTransactionId",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_AccountId_TransactionDate_Type",
                table: "FinancialTransactions",
                columns: new[] { "AccountId", "TransactionDate", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BuildingId",
                table: "FinancialTransactions",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ExpenseCategoryId",
                table: "FinancialTransactions",
                column: "ExpenseCategoryId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinancialTransactions_Amount",
                table: "FinancialTransactions",
                sql: "[Amount] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_AccountId_Type_Name",
                table: "ExpenseCategories",
                columns: new[] { "AccountId", "Type", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionReceipts_AccountId",
                table: "TransactionReceipts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionReceipts_FinancialTransactionId",
                table: "TransactionReceipts",
                column: "FinancialTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_Accounts_AccountId",
                table: "ExpenseCategories",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Accounts_AccountId",
                table: "FinancialTransactions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_BirdBuildings_BuildingId",
                table: "FinancialTransactions",
                column: "BuildingId",
                principalTable: "BirdBuildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_ExpenseCategories_ExpenseCategoryId",
                table: "FinancialTransactions",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_Accounts_AccountId",
                table: "ExpenseCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Accounts_AccountId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_BirdBuildings_BuildingId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_ExpenseCategories_ExpenseCategoryId",
                table: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "TransactionReceipts");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_AccountId_TransactionDate_Type",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_BuildingId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_ExpenseCategoryId",
                table: "FinancialTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinancialTransactions_Amount",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_AccountId_Type_Name",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "Counterparty",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ExpenseCategories");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "FinancialTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FinancialTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExpenseCategories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
