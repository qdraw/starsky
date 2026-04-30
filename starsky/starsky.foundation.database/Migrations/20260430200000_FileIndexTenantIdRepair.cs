using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    [DbContext(typeof(Data.ApplicationDbContext))]
    [Migration("20260430200000_FileIndexTenantIdRepair")]
    public partial class FileIndexTenantIdRepair : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO Tenants (Slug, Name, IsEnabled, Created)
SELECT 'main', 'main', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM Tenants WHERE Slug = 'main');");

            if (ActiveProvider.Contains("Sqlite"))
            {
                migrationBuilder.Sql(@"
UPDATE FileIndex
SET TenantId = (
    SELECT t.Id
    FROM Tenants t
    WHERE FileIndex.FilePath = '/' || t.Slug
       OR FileIndex.FilePath LIKE '/' || t.Slug || '/%'
    ORDER BY t.Id
    LIMIT 1
)
WHERE TenantId IS NULL
  AND EXISTS (
    SELECT 1
    FROM Tenants t
    WHERE FileIndex.FilePath = '/' || t.Slug
       OR FileIndex.FilePath LIKE '/' || t.Slug || '/%'
);");
            }
            else
            {
                migrationBuilder.Sql(@"
UPDATE FileIndex fi
JOIN Tenants t
  ON fi.FilePath = CONCAT('/', t.Slug)
  OR fi.FilePath LIKE CONCAT('/', t.Slug, '/%')
SET fi.TenantId = t.Id
WHERE fi.TenantId IS NULL;");
            }

            migrationBuilder.Sql(@"
UPDATE FileIndex
SET TenantId = (SELECT Id FROM Tenants WHERE Slug = 'main' LIMIT 1)
WHERE TenantId IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE FileIndex
SET TenantId = NULL
WHERE TenantId = (SELECT Id FROM Tenants WHERE Slug = 'main' LIMIT 1);");
        }
    }
}

