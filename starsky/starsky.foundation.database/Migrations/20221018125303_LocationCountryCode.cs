using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
	public partial class LocationCountryCode : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "LocationCountryCode",
				table: "FileIndex",
				type: "TEXT",
				maxLength: 3,
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "LocationCountryCode",
				table: "FileIndex");
		}
	}
}
