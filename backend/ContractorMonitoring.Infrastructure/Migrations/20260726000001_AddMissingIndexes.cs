using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorMonitoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UserRoles: standalone UserId index (composite unique already exists but no standalone)
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            // UserRoles: TenantId index for tenant-scoped queries
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_TenantId",
                table: "UserRoles",
                column: "TenantId");

            // AuditTrails: CreatedAt index for time-range queries
            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_CreatedAt",
                table: "AuditTrails",
                column: "CreatedAt");

            // AuditTrails: TenantId index for tenant-scoped chain verification
            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_TenantId",
                table: "AuditTrails",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_UserRoles_UserId", table: "UserRoles");
            migrationBuilder.DropIndex(name: "IX_UserRoles_TenantId", table: "UserRoles");
            migrationBuilder.DropIndex(name: "IX_AuditTrails_CreatedAt", table: "AuditTrails");
            migrationBuilder.DropIndex(name: "IX_AuditTrails_TenantId", table: "AuditTrails");
        }
    }
}
