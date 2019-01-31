﻿using Microsoft.EntityFrameworkCore.Migrations;
using starskycore.Attributes;

namespace starskycore.Migrations
{
    public partial class ColorClassFeature : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorClass",
                table: "FileIndex",
                nullable: false,
                defaultValue: 0);
        }

        [ExcludeFromCoverage]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorClass",
                table: "FileIndex");
        }
    }
}
