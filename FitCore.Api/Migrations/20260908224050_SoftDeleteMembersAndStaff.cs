using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteMembersAndStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Email",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Phone",
                table: "Members");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Staff",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_Email",
                table: "Members",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"Email\" IS NOT NULL AND \"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_Phone",
                table: "Members",
                columns: new[] { "TenantId", "Phone" },
                unique: true,
                filter: "\"Phone\" IS NOT NULL AND \"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Email",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Phone",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_Email",
                table: "Members",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_Phone",
                table: "Members",
                columns: new[] { "TenantId", "Phone" },
                unique: true,
                filter: "\"Phone\" IS NOT NULL");
        }
    }
}
