using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBirdNestSaleDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AveragePricePerKg",
                table: "FinancialTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaleLocation",
                table: "FinancialTransactions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AveragePricePerKg",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "SaleLocation",
                table: "FinancialTransactions");
        }
    }
}
