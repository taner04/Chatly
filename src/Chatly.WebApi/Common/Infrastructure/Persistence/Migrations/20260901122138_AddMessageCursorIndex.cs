using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageCursorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatId_SentAt",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId_SentAt_Id",
                table: "Messages",
                columns: new[] { "ChatId", "SentAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatId_SentAt_Id",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId_SentAt",
                table: "Messages",
                columns: new[] { "ChatId", "SentAt" });
        }
    }
}
