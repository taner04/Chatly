using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuth0IdToIdentityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Auth0Id",
                table: "Users",
                newName: "IdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Auth0Id",
                table: "Users",
                newName: "IX_Users_IdentityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdentityId",
                table: "Users",
                newName: "Auth0Id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_IdentityId",
                table: "Users",
                newName: "IX_Users_Auth0Id");
        }
    }
}
