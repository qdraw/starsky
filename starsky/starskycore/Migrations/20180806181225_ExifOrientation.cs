﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace starskycore.Migrations
{
    public partial class ExifOrientation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Orientation",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Orientation",
                table: "FileIndex");
        }
    }
}
