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
            migrationBuilder.DropIndex(
                name: "IX_FileIndex_ParentDirectory_FileName",
                table: "FileIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Thumbnails_Missing_And_FileHash",
                table: "Thumbnails",
                columns: new[] { "ExtraLarge", "Large", "Small", "FileHash" });

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_DateTime",
                table: "FileIndex",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_FileHash",
                table: "FileIndex",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_ParentDirectory_Tags",
                table: "FileIndex",
                columns: new[] { "ParentDirectory", "Tags" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_ParentDirectory_FileName",
                table: "FileIndex",
                columns: new[] { "ParentDirectory", "FileName" });
        }
    }
}
