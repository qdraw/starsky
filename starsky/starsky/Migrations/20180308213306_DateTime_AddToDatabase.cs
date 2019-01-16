﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;
using starsky.Attributes;
using starskycore.Attributes;

namespace starsky.Migrations
{
    public partial class DateTime_AddToDatabase : Migration
    {
        [ExcludeFromCoverage]
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

        [ExcludeFromCoverage]
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
