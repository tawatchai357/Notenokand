using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillBuildingAddressFromAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE building
                SET ProvinceCode = COALESCE(building.ProvinceCode, accountRow.ProvinceCode),
                    DistrictCode = COALESCE(building.DistrictCode, accountRow.DistrictCode),
                    SubdistrictCode = COALESCE(building.SubdistrictCode, accountRow.SubdistrictCode),
                    PostalCode = COALESCE(building.PostalCode, accountRow.PostalCode)
                FROM BirdBuildings AS building
                INNER JOIN Accounts AS accountRow ON accountRow.Id = building.AccountId
                WHERE building.ProvinceCode IS NULL
                   OR building.DistrictCode IS NULL
                   OR building.SubdistrictCode IS NULL
                   OR building.PostalCode IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
