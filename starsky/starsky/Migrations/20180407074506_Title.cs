using Microsoft.EntityFrameworkCore.Migrations;
using starsky.Attributes;
using starskycore.Attributes;

namespace starsky.Migrations
{
    public partial class Title : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "FileIndex",
                nullable: true);
        }

        [ExcludeFromCoverage]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "FileIndex");
        }
    }
}
