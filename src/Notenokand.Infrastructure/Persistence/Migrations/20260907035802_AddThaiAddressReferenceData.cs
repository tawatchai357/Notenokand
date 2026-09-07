using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddThaiAddressReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThaiProvinces",
                columns: table => new
                {
                    Code = table.Column<short>(type: "smallint", nullable: false),
                    NameTh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThaiProvinces", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ThaiDistricts",
                columns: table => new
                {
                    Code = table.Column<int>(type: "int", nullable: false),
                    ProvinceCode = table.Column<short>(type: "smallint", nullable: false),
                    NameTh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThaiDistricts", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ThaiDistricts_ThaiProvinces_ProvinceCode",
                        column: x => x.ProvinceCode,
                        principalTable: "ThaiProvinces",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThaiSubdistricts",
                columns: table => new
                {
                    Code = table.Column<int>(type: "int", nullable: false),
                    DistrictCode = table.Column<int>(type: "int", nullable: false),
                    NameTh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThaiSubdistricts", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ThaiSubdistricts_ThaiDistricts_DistrictCode",
                        column: x => x.DistrictCode,
                        principalTable: "ThaiDistricts",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThaiSubdistrictPostalCodes",
                columns: table => new
                {
                    SubdistrictCode = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "char(5)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThaiSubdistrictPostalCodes", x => new { x.SubdistrictCode, x.PostalCode });
                    table.ForeignKey(
                        name: "FK_ThaiSubdistrictPostalCodes_ThaiSubdistricts_SubdistrictCode",
                        column: x => x.SubdistrictCode,
                        principalTable: "ThaiSubdistricts",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThaiDistricts_ProvinceCode_NameTh",
                table: "ThaiDistricts",
                columns: new[] { "ProvinceCode", "NameTh" });

            migrationBuilder.CreateIndex(
                name: "IX_ThaiProvinces_NameTh",
                table: "ThaiProvinces",
                column: "NameTh");

            migrationBuilder.CreateIndex(
                name: "IX_ThaiSubdistrictPostalCodes_PostalCode",
                table: "ThaiSubdistrictPostalCodes",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_ThaiSubdistricts_DistrictCode_NameTh",
                table: "ThaiSubdistricts",
                columns: new[] { "DistrictCode", "NameTh" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThaiSubdistrictPostalCodes");

            migrationBuilder.DropTable(
                name: "ThaiSubdistricts");

            migrationBuilder.DropTable(
                name: "ThaiDistricts");

            migrationBuilder.DropTable(
                name: "ThaiProvinces");
        }
    }
}
