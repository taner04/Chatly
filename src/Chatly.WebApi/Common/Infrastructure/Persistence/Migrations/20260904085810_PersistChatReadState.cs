using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersistChatReadState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatReadStates",
                columns: table => new
                {
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatReadStates", x => new { x.ChatId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ChatReadStates_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatReadStates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatReadStates_UserId",
                table: "ChatReadStates",
                column: "UserId");

            migrationBuilder.Sql(
                """
                INSERT INTO "ChatReadStates" ("ChatId", "UserId", "LastReadAt")
                SELECT "ChatId", "UserId", MAX("SentAt")
                FROM (
                    SELECT c."Id" AS "ChatId", c."FirstUserId" AS "UserId", m."SentAt"
                    FROM "Chats" AS c
                    INNER JOIN "Messages" AS m ON m."ChatId" = c."Id"
                    UNION ALL
                    SELECT c."Id" AS "ChatId", c."SecondUserId" AS "UserId", m."SentAt"
                    FROM "Chats" AS c
                    INNER JOIN "Messages" AS m ON m."ChatId" = c."Id"
                ) AS existing_messages
                GROUP BY "ChatId", "UserId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatReadStates");
        }
    }
}
