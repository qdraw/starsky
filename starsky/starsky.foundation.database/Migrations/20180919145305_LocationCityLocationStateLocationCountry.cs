using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
    public partial class LocationCityLocationStateLocationCountry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationCity",
                table: "FileIndex",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCountry",
                table: "FileIndex",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationState",
                table: "FileIndex",
                maxLength: 40,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationCity",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "LocationCountry",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "LocationState",
                table: "FileIndex");
        }
    }
}
