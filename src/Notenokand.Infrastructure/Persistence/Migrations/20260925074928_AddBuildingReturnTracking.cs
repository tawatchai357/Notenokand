using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingReturnTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCapitalExpense",
                table: "FinancialTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialInvestmentAmount",
                table: "BirdBuildings",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCapitalExpense",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "InitialInvestmentAmount",
                table: "BirdBuildings");
        }
    }
}
