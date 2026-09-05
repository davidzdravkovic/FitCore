using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class OneUnusedPlatformLoginTokenPerAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlatformLoginTokens_PlatformAdminId",
                table: "PlatformLoginTokens");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformLoginTokens_OneUnusedPerAdmin",
                table: "PlatformLoginTokens",
                column: "PlatformAdminId",
                unique: true,
                filter: "\"UsedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlatformLoginTokens_OneUnusedPerAdmin",
                table: "PlatformLoginTokens");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformLoginTokens_PlatformAdminId",
                table: "PlatformLoginTokens",
                column: "PlatformAdminId");
        }
    }
}
