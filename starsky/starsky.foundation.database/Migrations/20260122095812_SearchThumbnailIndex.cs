using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class SearchThumbnailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        // Drop oversized indexes that exceed MariaDB's 3072 byte limit
	        // removed: IX_Thumbnails_Missing_And_FileHash
	        // 1. Thumbnails index with FileHash

	        // 2. ParentDirectory + Tags composite: 760 + 4096 = 4856 bytes
	        // removed: IX_FileIndex_ParentDirectory_Tags

	        // Create optimized indexes under 3072 byte limit
	        migrationBuilder.CreateIndex(
	            name: "IX_Thumbnails_Missing",
	            table: "Thumbnails",
	            columns: new[] { "ExtraLarge", "Large", "Small" });

	        migrationBuilder.CreateIndex(
	            name: "IX_FileIndex_ParentDirectory",
	            table: "FileIndex",
	            column: "ParentDirectory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.DropIndex(
		        name: "IX_Thumbnails_Missing",
		        table: "Thumbnails");

	        migrationBuilder.DropIndex(
		        name: "IX_FileIndex_ParentDirectory",
		        table: "FileIndex");
        }
    }
}

