using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class MembershipSessionBuckets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_Entitlement",
                table: "MembershipPlans");

            migrationBuilder.AddColumn<int>(
                name: "SessionTotal",
                table: "Memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionsBurned",
                table: "Memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionsReserved",
                table: "Memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Reserved = open scheduled visits that still hold a credit.
            migrationBuilder.Sql(
                """
                UPDATE "Memberships" AS m
                SET "SessionsReserved" = COALESCE((
                    SELECT COUNT(*)::int
                    FROM "Visits" AS v
                    WHERE v."MembershipId" = m."Id"
                      AND v."ConsumedSessionCredit" = TRUE
                      AND v."Status" = 'Scheduled'
                ), 0);
                """);

            // Total from plan when possible; else remaining + reserved.
            migrationBuilder.Sql(
                """
                UPDATE "Memberships" AS m
                SET "SessionTotal" = GREATEST(
                    COALESCE(p."SessionCount", 0),
                    COALESCE(m."SessionsRemaining", 0) + m."SessionsReserved"
                )
                FROM "MembershipPlans" AS p
                WHERE p."Id" = m."PlanId";
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Memberships" AS m
                SET "SessionsBurned" = GREATEST(
                    0,
                    m."SessionTotal" - COALESCE(m."SessionsRemaining", 0) - m."SessionsReserved"
                );
                """);

            // Time-period plans become session packs with a synthetic count so the new check passes.
            migrationBuilder.Sql(
                """
                UPDATE "MembershipPlans"
                SET "EntitlementType" = 'SessionPack',
                    "SessionCount" = COALESCE("SessionCount", "DurationDays", 1)
                WHERE "EntitlementType" = 'TimePeriod';
                """);

            migrationBuilder.DropColumn(
                name: "EndAt",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "SessionsRemaining",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "MembershipPlans");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Memberships_SessionBuckets",
                table: "Memberships",
                sql: "\"SessionTotal\" >= 0 AND \"SessionsReserved\" >= 0 AND \"SessionsBurned\" >= 0 AND \"SessionTotal\" >= (\"SessionsReserved\" + \"SessionsBurned\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_Entitlement",
                table: "MembershipPlans",
                sql: "\"EntitlementType\" = 'SessionPack' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Memberships_SessionBuckets",
                table: "Memberships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_Entitlement",
                table: "MembershipPlans");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndAt",
                table: "Memberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionsRemaining",
                table: "Memberships",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "MembershipPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Memberships"
                SET "SessionsRemaining" = GREATEST(0, "SessionTotal" - "SessionsReserved" - "SessionsBurned");
                """);

            migrationBuilder.DropColumn(
                name: "SessionTotal",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "SessionsBurned",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "SessionsReserved",
                table: "Memberships");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_Entitlement",
                table: "MembershipPlans",
                sql: "(\"EntitlementType\" = 'SessionPack' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0) OR (\"EntitlementType\" = 'TimePeriod' AND \"DurationDays\" IS NOT NULL AND \"DurationDays\" > 0)");
        }
    }
}
