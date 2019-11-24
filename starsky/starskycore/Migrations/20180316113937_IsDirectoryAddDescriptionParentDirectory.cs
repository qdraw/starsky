using Microsoft.EntityFrameworkCore.Migrations;
using starskycore.Attributes;

namespace starskycore.Migrations
{
    public partial class IsDirectoryAddDescriptionParentDirectory : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "ParentDirectory",
                table: "FileIndex",
                maxLength: 190,
                nullable: true);

            // SQLite does not support this migration operation ('RenameColumnOperation'). For more information, see http://go.microsoft.com/fwlink/?LinkId=723262.

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FileIndex",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDirectory",
                table: "FileIndex",
                nullable: false,
                defaultValue: false);
        }

        [ExcludeFromCoverage]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "IsDirectory",
                table: "FileIndex");

            // SQLite does not support this migration operation ('RenameColumnOperation'). For more information, see http://go.microsoft.com/fwlink/?LinkId=723262.

            migrationBuilder.DropColumn(
                name: "ParentDirectory",
                table: "FileIndex");

        }
    }
}
