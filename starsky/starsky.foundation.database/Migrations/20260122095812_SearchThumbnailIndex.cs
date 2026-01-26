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
	        // IX_Thumbnails_Missing_And_FileHash is removed here

			// Removed: Duplicate key name 'IX_FileIndex_DateTime'

			// Removed: Duplicate key name 'IX_FileIndex_FileHash'

			// removed: IX_FileIndex_ParentDirectory_Tags due to
			// "Specified key was too long; max key length is 3072 bytes"
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
	        try
	        {
		        migrationBuilder.DropIndex(
			        name: "IX_Thumbnails_Missing_And_FileHash",
			        table: "Thumbnails");
		        migrationBuilder.DropIndex(
			        name: "IX_FileIndex_DateTime",
			        table: "FileIndex");

		        migrationBuilder.DropIndex(
			        name: "IX_FileIndex_FileHash",
			        table: "FileIndex");

		        migrationBuilder.DropIndex(
			        name: "IX_FileIndex_ParentDirectory_Tags",
			        table: "FileIndex");
	        }
	        catch ( Exception )
	        {
		        // nothing here
	        }


        }
    }
}

