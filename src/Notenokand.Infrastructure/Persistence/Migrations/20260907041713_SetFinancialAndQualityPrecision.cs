using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetFinancialAndQualityPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalScore",
                table: "QualityScores",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxScore",
                table: "QualityCriteria",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumScore",
                table: "QualityBands",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumScore",
                table: "QualityBands",
                type: "decimal(9,3)",
                precision: 9,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DowntimeHours",
                table: "MaintenanceJobs",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalScore",
                table: "QualityScores",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)",
                oldPrecision: 9,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxScore",
                table: "QualityCriteria",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)",
                oldPrecision: 9,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumScore",
                table: "QualityBands",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)",
                oldPrecision: 9,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumScore",
                table: "QualityBands",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)",
                oldPrecision: 9,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "DowntimeHours",
                table: "MaintenanceJobs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);
        }
    }
}
