using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
    public partial class makeModelToImport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MakeModel",
                table: "ImportIndex",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MakeModel",
                table: "ImportIndex");
        }
    }
}
