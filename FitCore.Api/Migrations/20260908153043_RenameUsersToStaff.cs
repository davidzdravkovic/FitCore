using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations;

/// <inheritdoc />
public partial class RenameUsersToStaff : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Tenants_TenantId",
            table: "Users");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Users",
            table: "Users");

        migrationBuilder.RenameTable(
            name: "Users",
            newName: "Staff");

        migrationBuilder.RenameIndex(
            name: "IX_Users_TenantId_Email",
            table: "Staff",
            newName: "IX_Staff_TenantId_Email");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Staff",
            table: "Staff",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Staff_Tenants_TenantId",
            table: "Staff",
            column: "TenantId",
            principalTable: "Tenants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Staff_Tenants_TenantId",
            table: "Staff");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Staff",
            table: "Staff");

        migrationBuilder.RenameTable(
            name: "Staff",
            newName: "Users");

        migrationBuilder.RenameIndex(
            name: "IX_Staff_TenantId_Email",
            table: "Users",
            newName: "IX_Users_TenantId_Email");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Users",
            table: "Users",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Tenants_TenantId",
            table: "Users",
            column: "TenantId",
            principalTable: "Tenants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
