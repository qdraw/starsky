using Microsoft.EntityFrameworkCore.Migrations;
using starsky.Attributes;
using starskycore.Attributes;

namespace starsky.Migrations
{
    public partial class ColorClassFeature : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorClass",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0);
        }

        [ExcludeFromCoverage]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorClass",
                table: "FileIndex");
        }
    }
}
