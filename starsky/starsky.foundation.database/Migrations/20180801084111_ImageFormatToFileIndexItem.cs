using Microsoft.EntityFrameworkCore.Migrations;

namespace starskycore.Migrations
{
    public partial class ImageFormatToFileIndexItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageFormat",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFormat",
                table: "FileIndex");
        }
    }
}
