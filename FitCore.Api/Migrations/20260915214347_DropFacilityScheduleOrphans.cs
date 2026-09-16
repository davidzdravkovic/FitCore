using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class DropFacilityScheduleOrphans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TABLE IF EXISTS "Bookings";""");
            migrationBuilder.Sql("""DROP TABLE IF EXISTS "ClassSessions";""");
            migrationBuilder.Sql("""
                DELETE FROM "__EFMigrationsHistory"
                WHERE "MigrationId" = '20260915041758_AddScheduleSessionsAndBookings';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Facility schedule was abandoned; do not recreate orphan tables.
        }
    }
}
