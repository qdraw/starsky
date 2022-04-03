using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    public partial class DateTimeParsedFromFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DateTimeParsedFromFileName",
                table: "ImportIndex",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 380,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 380,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 190,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 190,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTimeParsedFromFileName",
                table: "ImportIndex");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 380,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 380,
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 190,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 190,
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 1);
        }
    }
}
