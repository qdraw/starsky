using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class QueueItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("MySql:ValueGeneratedOnAdd", true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QueueName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    JobId = table.Column<Guid>(type: "varchar(36)", maxLength: 36, nullable: false),
                    JobType = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    MetaData = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    TraceParentId = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PriorityLane = table.Column<int>(type: "INTEGER", nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClaimedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProcessedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_QueueItems_Queue_Status_Created",
                table: "QueueItems",
                columns: new[] { "QueueName", "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueItems");
        }
    }
}
