using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(Data.ApplicationDbContext))]
    [Migration("20260430121000_TenantIsolationPhase1Backfill")]
    public partial class TenantIsolationPhase1Backfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "QueueItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantSlug",
                table: "QueueItems",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "FileIndex",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_Tenant_FilePath",
                table: "FileIndex",
                columns: new[] { "TenantId", "FilePath" });

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_Tenant_Parent_FileName",
                table: "FileIndex",
                columns: new[] { "TenantId", "ParentDirectory", "FileName" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueItems_Tenant_Queue_Status_Created",
                table: "QueueItems",
                columns: new[] { "TenantId", "QueueName", "Status", "CreatedAtUtc" });

            migrationBuilder.Sql(@"
INSERT INTO Tenants (Slug, Name, IsEnabled, Created)
SELECT 'main', 'main', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM Tenants WHERE Slug = 'main');");

            migrationBuilder.Sql(@"
UPDATE FileIndex
SET TenantId = (SELECT Id FROM Tenants WHERE Slug = 'main' LIMIT 1)
WHERE TenantId IS NULL;");

            migrationBuilder.Sql(@"
UPDATE QueueItems
SET TenantId = (SELECT Id FROM Tenants WHERE Slug = 'main' LIMIT 1),
    TenantSlug = COALESCE(TenantSlug, 'main')
WHERE TenantId IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileIndex_Tenant_FilePath",
                table: "FileIndex");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_Tenant_Parent_FileName",
                table: "FileIndex");

            migrationBuilder.DropIndex(
                name: "IX_QueueItems_Tenant_Queue_Status_Created",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "TenantSlug",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FileIndex");
        }
    }
}


