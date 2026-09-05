using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformLoginTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformLoginTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformLoginTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformLoginTokens_PlatformAdmins_PlatformAdminId",
                        column: x => x.PlatformAdminId,
                        principalTable: "PlatformAdmins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformLoginTokens_PlatformAdminId",
                table: "PlatformLoginTokens",
                column: "PlatformAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformLoginTokens_TokenHash",
                table: "PlatformLoginTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformLoginTokens");
        }
    }
}
