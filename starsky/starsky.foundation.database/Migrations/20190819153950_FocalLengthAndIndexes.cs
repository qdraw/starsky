using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
	public partial class FocalLengthAndIndexes : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<double>(
				name: "FocalLength",
				table: "FileIndex",
				nullable: false,
				defaultValue: 0.0);

			migrationBuilder.CreateIndex(
				name: "IX_FileIndex_FileName_ParentDirectory",
				table: "FileIndex",
				columns: new[] { "FileName", "ParentDirectory" });
			// Specified key was too long; max key length is 3072 bytes
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_FileIndex_FileName_ParentDirectory",
				table: "FileIndex");

			migrationBuilder.DropColumn(
				name: "FocalLength",
				table: "FileIndex");
		}
	}
}
