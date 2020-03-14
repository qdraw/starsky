using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace starskycore.Migrations
{
    public partial class DateTime_AddToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddToDatabase",
                table: "FileIndex",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "FileIndex",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddToDatabase",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "FileIndex");
        }
    }
}
