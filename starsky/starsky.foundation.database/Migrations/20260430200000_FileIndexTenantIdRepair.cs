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

