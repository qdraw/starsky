using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class MultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.AddColumn<bool>(
                name: "IsPlatformAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastTenantId",
                table: "Users",
                type: "TEXT",
                maxLength: 63,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "UserRoles",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            // MySQL: the foreign key on UserId used the primary key as its index. Make the index
            // that stays (one role per user per tenant) before the primary key is replaced
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_TenantId",
                table: "UserRoles",
                columns: new[] { "UserId", "TenantId" },
                unique: true);

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Thumbnails",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Settings",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "QueueItems",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Notifications",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ImportIndex",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "FileIndex",
                type: "varchar(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "TenantId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails",
                columns: new[] { "TenantId", "FileHash" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                columns: new[] { "TenantId", "Key" });

            // existing installs: the current administrators may create tenants
            migrationBuilder.Sql(
                "UPDATE Users SET IsPlatformAdmin = 1 WHERE Id IN " +
                "(SELECT UserId FROM UserRoles WHERE RoleId IN " +
                "(SELECT Id FROM Roles WHERE Code = 'Administrator'))");

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    StorageFolder = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    ThumbnailTempFolder = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    TempFolder = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_TenantId",
                table: "UserRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_TenantId_FilePath",
                table: "FileIndex",
                columns: new[] { "TenantId", "FilePath" });

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_TenantId_ParentDirectory_FileName",
                table: "FileIndex",
                columns: new[] { "TenantId", "ParentDirectory", "FileName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");

            // the old key first, while the unique index still serves the foreign key on UserId
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_TenantId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_TenantId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_TenantId_FilePath",
                table: "FileIndex");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_TenantId_ParentDirectory_FileName",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "IsPlatformAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastTenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Thumbnails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ImportIndex");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FileIndex");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails",
                column: "FileHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                column: "Key");
        }
    }
}
