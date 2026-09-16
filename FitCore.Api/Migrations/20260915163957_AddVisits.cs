using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ConsumedSessionCredit = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.CheckConstraint("CK_Visits_EndAfterStart", "\"EndAt\" > \"StartAt\"");
                    table.ForeignKey(
                        name: "FK_Visits_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Staff_CoachStaffId",
                        column: x => x.CoachStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CoachStaffId",
                table: "Visits",
                column: "CoachStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_MemberId",
                table: "Visits",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_MembershipId",
                table: "Visits",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ServiceId",
                table: "Visits",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TenantId_CoachStaffId_StartAt",
                table: "Visits",
                columns: new[] { "TenantId", "CoachStaffId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TenantId_MemberId_StartAt",
                table: "Visits",
                columns: new[] { "TenantId", "MemberId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TenantId_MembershipId_Status",
                table: "Visits",
                columns: new[] { "TenantId", "MembershipId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TenantId_StartAt",
                table: "Visits",
                columns: new[] { "TenantId", "StartAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Visits");
        }
    }
}
