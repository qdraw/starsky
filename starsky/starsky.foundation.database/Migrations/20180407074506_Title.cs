using Microsoft.EntityFrameworkCore.Migrations;

namespace starskycore.Migrations
{
    public partial class Title : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "FileIndex",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "FileIndex");
        }
    }
}
