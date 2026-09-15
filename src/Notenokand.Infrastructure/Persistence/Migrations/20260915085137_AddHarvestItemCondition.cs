using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notenokand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHarvestItemCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestNestType' AND Code = '1') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110000-0000-4000-8000-000000000001','HarvestNestType','1',N'รังถ้วย',1,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestNestType' AND Code = '2') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110000-0000-4000-8000-000000000002','HarvestNestType','2',N'รังกะเทย',2,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestNestType' AND Code = '3') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110000-0000-4000-8000-000000000003','HarvestNestType','3',N'รังมุม',3,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestNestType' AND Code = '4') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110000-0000-4000-8000-000000000004','HarvestNestType','4',N'รังแตกหัก',4,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestNestType' AND Code = '5') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110000-0000-4000-8000-000000000005','HarvestNestType','5',N'หักผิดรูปทรง',5,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestNestType' AND Code = '6') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110000-0000-4000-8000-000000000006','HarvestNestType','6',N'เศษผล',6,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestColor' AND Code = '1') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110001-0000-4000-8000-000000000001','HarvestColor','1',N'รังขาว',1,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestColor' AND Code = '2') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110001-0000-4000-8000-000000000002','HarvestColor','2',N'รังเหลือง',2,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestColor' AND Code = '3') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110001-0000-4000-8000-000000000003','HarvestColor','3',N'รังเทา',3,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestCondition' AND Code = '1') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110002-0000-4000-8000-000000000001','HarvestCondition','1',N'ปกติ',1,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestCondition' AND Code = '2') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110002-0000-4000-8000-000000000002','HarvestCondition','2',N'ท้องรังดำ',2,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestCondition' AND Code = '3') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110002-0000-4000-8000-000000000003','HarvestCondition','3',N'รา',3,1,SYSDATETIMEOFFSET(),0);
IF NOT EXISTS (SELECT 1 FROM MasterOptions WHERE Category = 'HarvestCondition' AND Code = '4') INSERT INTO MasterOptions (Id,Category,Code,Name,SortOrder,IsActive,CreatedAt,IsDeleted) VALUES ('a3110002-0000-4000-8000-000000000004','HarvestCondition','4',N'สกปรกขี้ตก',4,1,SYSDATETIMEOFFSET(),0);
""");
            migrationBuilder.AddColumn<Guid>(
                name: "ConditionId",
                table: "HarvestItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HarvestItems_ConditionId",
                table: "HarvestItems",
                column: "ConditionId");

            migrationBuilder.AddForeignKey(
                name: "FK_HarvestItems_MasterOptions_ConditionId",
                table: "HarvestItems",
                column: "ConditionId",
                principalTable: "MasterOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HarvestItems_MasterOptions_ConditionId",
                table: "HarvestItems");

            migrationBuilder.DropIndex(
                name: "IX_HarvestItems_ConditionId",
                table: "HarvestItems");

            migrationBuilder.DropColumn(
                name: "ConditionId",
                table: "HarvestItems");
        }
    }
}
