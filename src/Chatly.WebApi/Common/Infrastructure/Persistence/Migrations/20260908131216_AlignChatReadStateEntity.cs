using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignChatReadStateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatReadStates",
                table: "ChatReadStates");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChatReadStates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ChatReadStates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ChatReadStates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ChatReadStates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ChatReadStates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "ChatReadStates"
                SET "Id" = gen_random_uuid(),
                    "CreatedAt" = "LastReadAt",
                    "CreatedBy" = 'system';
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChatReadStates",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ChatReadStates",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ChatReadStates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatReadStates",
                table: "ChatReadStates",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatReadStates_ChatId_UserId",
                table: "ChatReadStates",
                columns: new[] { "ChatId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatReadStates",
                table: "ChatReadStates");

            migrationBuilder.DropIndex(
                name: "IX_ChatReadStates_ChatId_UserId",
                table: "ChatReadStates");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChatReadStates");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ChatReadStates");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ChatReadStates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ChatReadStates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ChatReadStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatReadStates",
                table: "ChatReadStates",
                columns: new[] { "ChatId", "UserId" });
        }
    }
}
