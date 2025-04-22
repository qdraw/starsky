using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class imageformattagsindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_ImageFormat",
                table: "FileIndex",
                column: "ImageFormat");

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_Tags",
                table: "FileIndex",
                column: "Tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileIndex_ImageFormat",
                table: "FileIndex");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_Tags",
                table: "FileIndex");
        }
    }
}
