using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredFilesAndMessageAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePictureFileId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                    table.CheckConstraint("CK_StoredFiles_NonNegativeSize", "\"Size\" >= 0");
                    table.ForeignKey(
                        name: "FK_StoredFiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO "StoredFiles" (
                    "Id",
                    "UserId",
                    "BlobName",
                    "FileName",
                    "ContentType",
                    "Size",
                    "CreatedAt",
                    "CreatedBy")
                SELECT
                    gen_random_uuid(),
                    "Id",
                    "ProfilePictureKey",
                    regexp_replace("ProfilePictureKey", '^.*/', ''),
                    CASE
                        WHEN lower("ProfilePictureKey") LIKE '%.jpg' OR lower("ProfilePictureKey") LIKE '%.jpeg' THEN 'image/jpeg'
                        WHEN lower("ProfilePictureKey") LIKE '%.png' THEN 'image/png'
                        WHEN lower("ProfilePictureKey") LIKE '%.gif' THEN 'image/gif'
                        WHEN lower("ProfilePictureKey") LIKE '%.webp' THEN 'image/webp'
                        ELSE 'application/octet-stream'
                    END,
                    0,
                    "CreatedAt",
                    "CreatedBy"
                FROM "Users"
                WHERE "ProfilePictureKey" IS NOT NULL;

                UPDATE "Users" AS users
                SET "ProfilePictureFileId" = files."Id"
                FROM "StoredFiles" AS files
                WHERE users."Id" = files."UserId"
                  AND users."ProfilePictureKey" = files."BlobName";
                """);

            migrationBuilder.DropColumn(
                name: "ProfilePictureKey",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "MessageAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageAttachments_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageAttachments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProfilePictureFileId",
                table: "Users",
                column: "ProfilePictureFileId",
                unique: true,
                filter: "\"ProfilePictureFileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_MessageId_StoredFileId",
                table: "MessageAttachments",
                columns: new[] { "MessageId", "StoredFileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_StoredFileId",
                table: "MessageAttachments",
                column: "StoredFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_BlobName",
                table: "StoredFiles",
                column: "BlobName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UserId",
                table: "StoredFiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_StoredFiles_ProfilePictureFileId",
                table: "Users",
                column: "ProfilePictureFileId",
                principalTable: "StoredFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_StoredFiles_ProfilePictureFileId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureKey",
                table: "Users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Users" AS users
                SET "ProfilePictureKey" = files."BlobName"
                FROM "StoredFiles" AS files
                WHERE users."ProfilePictureFileId" = files."Id";
                """);

            migrationBuilder.DropTable(
                name: "MessageAttachments");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProfilePictureFileId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePictureFileId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Messages");
        }
    }
}
