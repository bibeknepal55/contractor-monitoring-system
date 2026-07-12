using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorMonitoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DeviceInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    RequestMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RequestUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    ResponseStatus = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LoginTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LogoutTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SessionDuration = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_ActivityType",
                table: "UserActivityLogs",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_ActivityType_CreatedAt",
                table: "UserActivityLogs",
                columns: new[] { "ActivityType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_CreatedAt",
                table: "UserActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_IpAddress",
                table: "UserActivityLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_ModuleName",
                table: "UserActivityLogs",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_ModuleName_ActivityType",
                table: "UserActivityLogs",
                columns: new[] { "ModuleName", "ActivityType" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_ResponseStatus",
                table: "UserActivityLogs",
                column: "ResponseStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_SessionId",
                table: "UserActivityLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_TenantId",
                table: "UserActivityLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_UserId",
                table: "UserActivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_UserId_CreatedAt",
                table: "UserActivityLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivityLogs");
        }
    }
}
