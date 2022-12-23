using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    public partial class ThumbnailSizes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailSizes",
                table: "FileIndex",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailSizes",
                table: "FileIndex");
        }
    }
}
