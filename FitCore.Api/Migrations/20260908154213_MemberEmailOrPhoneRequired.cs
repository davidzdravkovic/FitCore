using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class MemberEmailOrPhoneRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_EmailOrPhone",
                table: "Members",
                sql: "(\"Email\" IS NOT NULL AND btrim(\"Email\") <> '') OR (\"Phone\" IS NOT NULL AND btrim(\"Phone\") <> '')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_EmailOrPhone",
                table: "Members");
        }
    }
}
