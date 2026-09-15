using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class MemberCancelledReplacesDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Members"
                SET "Status" = 'Cancelled'
                WHERE "DeletedAt" IS NOT NULL;

                UPDATE "Memberships" m
                SET "Status" = 'Frozen'
                FROM "Members" mem
                WHERE m."MemberId" = mem."Id"
                  AND mem."DeletedAt" IS NOT NULL
                  AND m."Status" = 'Active';
                """);

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Email",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Phone",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_Email",
                table: "Members",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"Email\" IS NOT NULL AND \"Status\" <> 'Cancelled'");

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_Phone",
                table: "Members",
                columns: new[] { "TenantId", "Phone" },
                unique: true,
                filter: "\"Phone\" IS NOT NULL AND \"Status\" <> 'Cancelled'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Email",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_Phone",
                table: "Members");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Members",
                type: "timestamp with time zone",
                nullable: true);

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

            migrationBuilder.Sql("""
                UPDATE "Members"
                SET "DeletedAt" = NOW() AT TIME ZONE 'utc'
                WHERE "Status" = 'Cancelled' AND "DeletedAt" IS NULL;
                """);
        }
    }
}
