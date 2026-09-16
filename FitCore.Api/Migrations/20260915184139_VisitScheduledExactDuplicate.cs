using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class VisitScheduledExactDuplicate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Visits_Scheduled_ExactDuplicate",
                table: "Visits",
                columns: new[] { "TenantId", "MembershipId", "CoachStaffId", "StartAt", "EndAt" },
                unique: true,
                filter: "\"Status\" = 'Scheduled'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_Scheduled_ExactDuplicate",
                table: "Visits");
        }
    }
}
