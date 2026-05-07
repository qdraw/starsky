using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class QueueItemsTableAddCustomAiTags : Migration
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

            migrationBuilder.CreateTable(
                name: "QueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QueueName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    JobId = table.Column<Guid>(type: "varchar(36)", maxLength: 36, nullable: false),
                    JobType = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    MetaData = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    TraceParentId = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PriorityLane = table.Column<int>(type: "INTEGER", nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", maxLength: 27, nullable: false),
                    ClaimedAtUtc = table.Column<DateTime>(type: "TEXT", maxLength: 27, nullable: true),
                    ProcessedAtUtc = table.Column<DateTime>(type: "TEXT", maxLength: 27, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueueItems_JobId",
                table: "QueueItems",
                column: "JobId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueItems");

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
