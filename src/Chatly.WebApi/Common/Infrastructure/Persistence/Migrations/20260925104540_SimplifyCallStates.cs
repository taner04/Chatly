using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyCallStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Calls_AcceptedAt",
                table: "Calls");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Calls_ValidStatus",
                table: "Calls");

            migrationBuilder.Sql("UPDATE \"Calls\" SET \"Status\" = 'Active' WHERE \"Status\" IN ('Accepted', 'Offered');");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Calls_AcceptedAt",
                table: "Calls",
                sql: "(\"Status\" = 'Ringing' AND \"AcceptedAt\" IS NULL) OR (\"Status\" = 'Active' AND \"AcceptedAt\" IS NOT NULL) OR \"Status\" = 'Ended'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Calls_ValidStatus",
                table: "Calls",
                sql: "\"Status\" IN ('Ringing', 'Active', 'Ended')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Calls_AcceptedAt",
                table: "Calls");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Calls_ValidStatus",
                table: "Calls");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Calls_AcceptedAt",
                table: "Calls",
                sql: "(\"Status\" = 'Ringing' AND \"AcceptedAt\" IS NULL) OR (\"Status\" IN ('Accepted', 'Offered', 'Active') AND \"AcceptedAt\" IS NOT NULL) OR \"Status\" = 'Ended'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Calls_ValidStatus",
                table: "Calls",
                sql: "\"Status\" IN ('Ringing', 'Accepted', 'Offered', 'Active', 'Ended')");
        }
    }
}
