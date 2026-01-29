using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoNameCities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeoNameCities",
                columns: table => new
                {
                    GeonameId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AsciiName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AlternateNames = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    FeatureClass = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    FeatureCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Cc2 = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Admin1Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Admin2Code = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Admin3Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Admin4Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Population = table.Column<long>(type: "INTEGER", nullable: false),
                    Elevation = table.Column<int>(type: "INTEGER", nullable: true),
                    Dem = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ModificationDate = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoNameCities", x => x.GeonameId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeoNameCities");
        }
    }
}
