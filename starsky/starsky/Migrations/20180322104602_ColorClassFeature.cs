using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace starsky.Migrations
{
    public partial class ColorClassFeature : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorClass",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorClass",
                table: "FileIndex");
        }
    }
}
