using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreserveDirectChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Friendships_FriendshipId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_FriendshipId",
                table: "Chats");

            migrationBuilder.AddColumn<Guid>(
                name: "FirstUserId",
                table: "Chats",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondUserId",
                table: "Chats",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Chats" AS chat
                SET "FirstUserId" = friendship."FirstUserId",
                    "SecondUserId" = friendship."SecondUserId"
                FROM "Friendships" AS friendship
                WHERE chat."FriendshipId" = friendship."Id";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "FirstUserId",
                table: "Chats",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SecondUserId",
                table: "Chats",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "FriendshipId",
                table: "Chats");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_FirstUserId_SecondUserId",
                table: "Chats",
                columns: new[] { "FirstUserId", "SecondUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SecondUserId",
                table: "Chats",
                column: "SecondUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Chats_DistinctUsers",
                table: "Chats",
                sql: "\"FirstUserId\" <> \"SecondUserId\"");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_FirstUserId",
                table: "Chats",
                column: "FirstUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_SecondUserId",
                table: "Chats",
                column: "SecondUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_FirstUserId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_SecondUserId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_FirstUserId_SecondUserId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_SecondUserId",
                table: "Chats");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Chats_DistinctUsers",
                table: "Chats");

            migrationBuilder.AddColumn<Guid>(
                name: "FriendshipId",
                table: "Chats",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Chats" AS chat
                SET "FriendshipId" = friendship."Id"
                FROM "Friendships" AS friendship
                WHERE chat."FirstUserId" = friendship."FirstUserId"
                  AND chat."SecondUserId" = friendship."SecondUserId";

                DELETE FROM "Chats" WHERE "FriendshipId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "FriendshipId",
                table: "Chats",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "FirstUserId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "SecondUserId",
                table: "Chats");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_FriendshipId",
                table: "Chats",
                column: "FriendshipId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Friendships_FriendshipId",
                table: "Chats",
                column: "FriendshipId",
                principalTable: "Friendships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
