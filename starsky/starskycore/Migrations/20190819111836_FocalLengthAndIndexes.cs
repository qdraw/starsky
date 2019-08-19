using Microsoft.EntityFrameworkCore.Migrations;

namespace starskycore.Migrations
{
    public partial class FocalLengthAndIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FocalLength",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_FileHash_FilePath_FileName_Tags_ParentDirectory",
                table: "FileIndex",
                columns: new[] { "FileHash", "FilePath", "FileName", "Tags", "ParentDirectory" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileIndex_FileHash_FilePath_FileName_Tags_ParentDirectory",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "FocalLength",
                table: "FileIndex");
        }
    }
}
