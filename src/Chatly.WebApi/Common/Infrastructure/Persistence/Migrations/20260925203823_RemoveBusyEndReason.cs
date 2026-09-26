using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBusyEndReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Calls_ValidEndReason",
                table: "Calls");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Calls_ValidEndReason",
                table: "Calls",
                sql: "\"EndReason\" IS NULL OR \"EndReason\" IN ('Completed', 'Declined', 'Cancelled', 'Missed', 'Failed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Calls_ValidEndReason",
                table: "Calls");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Calls_ValidEndReason",
                table: "Calls",
                sql: "\"EndReason\" IS NULL OR \"EndReason\" IN ('Completed', 'Declined', 'Cancelled', 'Missed', 'Busy', 'Failed')");
        }
    }
}
