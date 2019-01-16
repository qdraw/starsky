using Microsoft.EntityFrameworkCore.Migrations;
using starsky.Attributes;
using starskycore.Attributes;

namespace starsky.Migrations
{
    public partial class LatitudeLongitudeGPS : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0.0);
        }
        
        [ExcludeFromCoverage]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "FileIndex");
        }
    }
}
