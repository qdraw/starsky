using Microsoft.EntityFrameworkCore.Migrations;

namespace starskycore.Migrations
{
    public partial class AddApertureShutterSpeedIsoSpeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Aperture",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<ushort>(
                name: "IsoSpeed",
                table: "FileIndex",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<string>(
                name: "ShutterSpeed",
                table: "FileIndex",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aperture",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "IsoSpeed",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "ShutterSpeed",
                table: "FileIndex");
        }
    }
}
