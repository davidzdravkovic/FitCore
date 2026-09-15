using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class MembershipCancelAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelNote",
                table: "Memberships",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Memberships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Memberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByStaffId",
                table: "Memberships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_CancelledByStaffId",
                table: "Memberships",
                column: "CancelledByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Staff_CancelledByStaffId",
                table: "Memberships",
                column: "CancelledByStaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Staff_CancelledByStaffId",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_CancelledByStaffId",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CancelNote",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "CancelledByStaffId",
                table: "Memberships");
        }
    }
}
