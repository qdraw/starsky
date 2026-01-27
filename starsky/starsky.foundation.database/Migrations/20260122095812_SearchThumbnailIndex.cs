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
	        
	        migrationBuilder.CreateIndex(
		        name: "IX_FileIndex_DateTime",
		        table: "FileIndex",
		        column: "DateTime");

	        migrationBuilder.CreateIndex(
		        name: "IX_FileIndex_FileHash",
		        table: "FileIndex",
		        column: "FileHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.DropIndex(
		        name: "IX_FileIndex_DateTime",
		        table: "FileIndex");

	        migrationBuilder.DropIndex(
		        name: "IX_FileIndex_FileHash",
		        table: "FileIndex");
        }
    }
}

