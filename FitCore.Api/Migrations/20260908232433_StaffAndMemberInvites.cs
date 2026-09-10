using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffAndMemberInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Staff rows that existed before roles were introduced are gym owners.
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Staff",
                type: "text",
                nullable: false,
                defaultValue: "Owner");

            migrationBuilder.CreateTable(
                name: "MemberInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberInvites_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberInvites_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffInvites_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffInvites_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvites_OneUnusedPerMember",
                table: "MemberInvites",
                column: "MemberId",
                unique: true,
                filter: "\"UsedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvites_TenantId",
                table: "MemberInvites",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvites_TokenHash",
                table: "MemberInvites",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvites_OneUnusedPerStaff",
                table: "StaffInvites",
                column: "StaffId",
                unique: true,
                filter: "\"UsedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvites_TenantId",
                table: "StaffInvites",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvites_TokenHash",
                table: "StaffInvites",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberInvites");

            migrationBuilder.DropTable(
                name: "StaffInvites");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Staff");
        }
    }
}
