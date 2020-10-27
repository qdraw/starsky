using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
    public partial class SideCarPathsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SidecarPaths",
                table: "FileIndex",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SidecarPaths",
                table: "FileIndex");
        }
    }
}
