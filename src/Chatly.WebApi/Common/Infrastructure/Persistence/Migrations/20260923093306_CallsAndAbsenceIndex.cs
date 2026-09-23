using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatly.WebApi.Common.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CallsAndAbsenceIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Calls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EndReason = table.Column<string>(type: "text", nullable: true),
                    InitiatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calls", x => x.Id);
                    table.CheckConstraint("CK_Calls_AcceptedAt", "(\"Status\" = 'Ringing' AND \"AcceptedAt\" IS NULL) OR (\"Status\" IN ('Accepted', 'Offered', 'Active') AND \"AcceptedAt\" IS NOT NULL) OR \"Status\" = 'Ended'");
                    table.CheckConstraint("CK_Calls_DistinctUsers", "\"CallerUserId\" <> \"ReceiverUserId\"");
                    table.CheckConstraint("CK_Calls_TerminalState", "(\"Status\" = 'Ended' AND \"EndedAt\" IS NOT NULL AND \"EndReason\" IS NOT NULL) OR (\"Status\" <> 'Ended' AND \"EndedAt\" IS NULL AND \"EndReason\" IS NULL)");
                    table.CheckConstraint("CK_Calls_TimestampOrder", "(\"AcceptedAt\" IS NULL OR \"AcceptedAt\" >= \"InitiatedAt\") AND (\"EndedAt\" IS NULL OR \"EndedAt\" >= \"InitiatedAt\") AND (\"AcceptedAt\" IS NULL OR \"EndedAt\" IS NULL OR \"EndedAt\" >= \"AcceptedAt\")");
                    table.CheckConstraint("CK_Calls_ValidEndReason", "\"EndReason\" IS NULL OR \"EndReason\" IN ('Completed', 'Declined', 'Cancelled', 'Missed', 'Busy', 'Failed')");
                    table.CheckConstraint("CK_Calls_ValidStatus", "\"Status\" IN ('Ringing', 'Accepted', 'Offered', 'Active', 'Ended')");
                    table.ForeignKey(
                        name: "FK_Calls_Users_CallerUserId",
                        column: x => x.CallerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Calls_Users_ReceiverUserId",
                        column: x => x.ReceiverUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActiveCallParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveCallParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveCallParticipants_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveCallParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"LastSeenAt\" = \"CreatedAt\" WHERE \"LastSeenAt\" IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastSeenAt_LastAbsenceEmailAt",
                table: "Users",
                columns: new[] { "LastSeenAt", "LastAbsenceEmailAt" },
                filter: "\"OnboardingCompleted\" = TRUE AND \"LastSeenAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveCallParticipants_CallId_UserId",
                table: "ActiveCallParticipants",
                columns: new[] { "CallId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActiveCallParticipants_UserId",
                table: "ActiveCallParticipants",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calls_CallerUserId_InitiatedAt",
                table: "Calls",
                columns: new[] { "CallerUserId", "InitiatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Calls_ReceiverUserId_InitiatedAt",
                table: "Calls",
                columns: new[] { "ReceiverUserId", "InitiatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveCallParticipants");

            migrationBuilder.DropTable(
                name: "Calls");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastSeenAt_LastAbsenceEmailAt",
                table: "Users");
        }
    }
}
