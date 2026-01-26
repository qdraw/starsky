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

	        // 1. Thumbnails index with FileHash
	        try
	        {
		        migrationBuilder.DropIndex(
			        name: "IX_Thumbnails_Missing_And_FileHash",
			        table: "Thumbnails");
	        }
	        catch ( Exception )
	        {
		        // Index might not exist
	        }

	        // 2. Tags index: varchar(1024) = 4096 bytes
	        try
	        {
		        migrationBuilder.DropIndex(
			        name: "IX_FileIndexItem_Tags",
			        table: "FileIndex");
	        }
	        catch ( Exception )
	        {
		        // Index might not exist
	        }

	        // 3. ParentDirectory + Tags composite: 760 + 4096 = 4856 bytes
	        try
	        {
		        migrationBuilder.DropIndex(
			        name: "IX_FileIndex_ParentDirectory_Tags",
			        table: "FileIndex");
	        }
	        catch ( Exception )
	        {
		        // Index might not exist
	        }

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
	        // Drop the optimized indexes
	        try
	        {
		        migrationBuilder.DropIndex(
			        name: "IX_Thumbnails_Missing",
			        table: "Thumbnails");

		        migrationBuilder.DropIndex(
			        name: "IX_FileIndex_ParentDirectory",
			        table: "FileIndex");
	        }
	        catch ( Exception )
	        {
		        // Indexes might not exist
	        }

	        // Recreate old indexes (note: these may fail due to size limits)
	        try
	        {
		        migrationBuilder.CreateIndex(
			        name: "IX_Thumbnails_Missing_And_FileHash",
			        table: "Thumbnails",
			        columns: new[] { "ExtraLarge", "Large", "Small", "FileHash" });

		        migrationBuilder.CreateIndex(
			        name: "IX_FileIndexItem_Tags",
			        table: "FileIndex",
			        column: "Tags");

		        migrationBuilder.CreateIndex(
			        name: "IX_FileIndex_ParentDirectory_Tags",
			        table: "FileIndex",
			        columns: new[] { "ParentDirectory", "Tags" });
	        }
	        catch ( Exception )
	        {
		        // Index creation may fail if it exceeds size limit
	        }

        }
    }
}

