using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class IndexFixThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        try
	        {
		        migrationBuilder.DropIndex(
			        name: "IX_Thumbnails_Missing_And_FileHash",
			        table: "Thumbnails");
	        }
	        catch (Exception)
	        {
		        // nothing here
	        }

            migrationBuilder.CreateIndex(
                name: "IX_Thumbnails_Missing",
                table: "Thumbnails",
                columns: new[] { "ExtraLarge", "Large", "Small" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Thumbnails_Missing",
                table: "Thumbnails");
            
        }
    }
}
