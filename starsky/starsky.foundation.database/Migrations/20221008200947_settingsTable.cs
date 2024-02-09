using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
	public partial class settingsTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Settings",
				columns: table => new
				{
					Key = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
					Value = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
					IsUserEditable = table.Column<bool>(type: "INTEGER", nullable: false),
					UserId = table.Column<int>(type: "INTEGER", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Settings", x => x.Key);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Settings");
		}
	}
}
