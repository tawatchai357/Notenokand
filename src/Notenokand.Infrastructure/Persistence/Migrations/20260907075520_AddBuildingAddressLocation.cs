using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingAddressLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistrictCode",
                table: "BirdBuildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "BirdBuildings",
                type: "char(5)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "ProvinceCode",
                table: "BirdBuildings",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubdistrictCode",
                table: "BirdBuildings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BirdBuildings_DistrictCode",
                table: "BirdBuildings",
                column: "DistrictCode");

            migrationBuilder.CreateIndex(
                name: "IX_BirdBuildings_ProvinceCode",
                table: "BirdBuildings",
                column: "ProvinceCode");

            migrationBuilder.CreateIndex(
                name: "IX_BirdBuildings_SubdistrictCode",
                table: "BirdBuildings",
                column: "SubdistrictCode");

            migrationBuilder.AddForeignKey(
                name: "FK_BirdBuildings_ThaiDistricts_DistrictCode",
                table: "BirdBuildings",
                column: "DistrictCode",
                principalTable: "ThaiDistricts",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BirdBuildings_ThaiProvinces_ProvinceCode",
                table: "BirdBuildings",
                column: "ProvinceCode",
                principalTable: "ThaiProvinces",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BirdBuildings_ThaiSubdistricts_SubdistrictCode",
                table: "BirdBuildings",
                column: "SubdistrictCode",
                principalTable: "ThaiSubdistricts",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BirdBuildings_ThaiDistricts_DistrictCode",
                table: "BirdBuildings");

            migrationBuilder.DropForeignKey(
                name: "FK_BirdBuildings_ThaiProvinces_ProvinceCode",
                table: "BirdBuildings");

            migrationBuilder.DropForeignKey(
                name: "FK_BirdBuildings_ThaiSubdistricts_SubdistrictCode",
                table: "BirdBuildings");

            migrationBuilder.DropIndex(
                name: "IX_BirdBuildings_DistrictCode",
                table: "BirdBuildings");

            migrationBuilder.DropIndex(
                name: "IX_BirdBuildings_ProvinceCode",
                table: "BirdBuildings");

            migrationBuilder.DropIndex(
                name: "IX_BirdBuildings_SubdistrictCode",
                table: "BirdBuildings");

            migrationBuilder.DropColumn(
                name: "DistrictCode",
                table: "BirdBuildings");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "BirdBuildings");

            migrationBuilder.DropColumn(
                name: "ProvinceCode",
                table: "BirdBuildings");

            migrationBuilder.DropColumn(
                name: "SubdistrictCode",
                table: "BirdBuildings");
        }
    }
}
