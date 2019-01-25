using Microsoft.EntityFrameworkCore.Migrations;
using starskycore.Attributes;

namespace starskycore.Migrations
{
    public partial class FileHashTags : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "FileIndex",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "FileIndex",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "FileIndex");
        }
    }
}
