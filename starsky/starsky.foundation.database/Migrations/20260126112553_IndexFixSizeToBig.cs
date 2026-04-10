using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class IndexFixSizeToBig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Can't drop IX_FileIndex_ParentDirectory_Tags

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

            // Can't create IX_FileIndex_ParentDirectory_Tags
        }
    }
}
