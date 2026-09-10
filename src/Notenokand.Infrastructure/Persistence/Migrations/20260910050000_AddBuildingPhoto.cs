using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Notenokand.Infrastructure.Persistence;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations;

[DbContext(typeof(NotenokandDbContext))]
[Migration("20260910050000_AddBuildingPhoto")]
public partial class AddBuildingPhoto : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "PhotoContentType", table: "BirdBuildings", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PhotoOriginalFileName", table: "BirdBuildings", type: "nvarchar(255)", maxLength: 255, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PhotoSha256", table: "BirdBuildings", type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: true);
        migrationBuilder.AddColumn<long>(name: "PhotoSizeBytes", table: "BirdBuildings", type: "bigint", nullable: true);
        migrationBuilder.AddColumn<string>(name: "PhotoStorageKey", table: "BirdBuildings", type: "nvarchar(500)", maxLength: 500, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PhotoContentType", table: "BirdBuildings");
        migrationBuilder.DropColumn(name: "PhotoOriginalFileName", table: "BirdBuildings");
        migrationBuilder.DropColumn(name: "PhotoSha256", table: "BirdBuildings");
        migrationBuilder.DropColumn(name: "PhotoSizeBytes", table: "BirdBuildings");
        migrationBuilder.DropColumn(name: "PhotoStorageKey", table: "BirdBuildings");
    }
}