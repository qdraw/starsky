using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace starsky.Migrations
{
    public partial class FileHashTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "FileIndex",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "FileIndex",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "FileIndex");
        }
    }
}
