using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class VisitVoidAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoidNote",
                table: "Visits",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAt",
                table: "Visits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoidedByStaffId",
                table: "Visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_VoidedByStaffId",
                table: "Visits",
                column: "VoidedByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Staff_VoidedByStaffId",
                table: "Visits",
                column: "VoidedByStaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Staff_VoidedByStaffId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_VoidedByStaffId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VoidNote",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VoidedAt",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VoidedByStaffId",
                table: "Visits");
        }
    }
}
