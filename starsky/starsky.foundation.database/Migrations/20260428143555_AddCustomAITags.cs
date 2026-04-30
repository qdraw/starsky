using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomAITags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ImageClassificationGeneratedAt",
                table: "FileIndex",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ImageClassificationModel",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedTags",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedTags",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageClassificationGeneratedAt",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "ImageClassificationModel",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "RejectedTags",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "SuggestedTags",
                table: "FileIndex");
        }
    }
}
