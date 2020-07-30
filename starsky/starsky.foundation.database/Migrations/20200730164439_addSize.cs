using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
    public partial class addSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDirectory",
                table: "FileIndex",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

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

            migrationBuilder.AlterColumn<bool>(
                name: "IsDirectory",
                table: "FileIndex",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }
    }
}
