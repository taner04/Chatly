using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandReactionTypeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reactions_ValidType",
                table: "Reactions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reactions_ValidType",
                table: "Reactions",
                sql: "\"Type\" IN ('Like', 'Love', 'Laugh', 'Surprised', 'Sad', 'Angry', 'Dislike', 'Smile', 'Fire', 'Celebrate', 'Clap', 'Thanks')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reactions_ValidType",
                table: "Reactions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reactions_ValidType",
                table: "Reactions",
                sql: "\"Type\" IN ('Like', 'Love', 'Laugh', 'Surprised', 'Sad', 'Angry')");
        }
    }
}
