using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
    public partial class addSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "FileIndex");
        }
    }
}
