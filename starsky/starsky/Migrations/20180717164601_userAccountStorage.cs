using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace starsky.Migrations
{
    public partial class userAccountStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // here
//
//            migrationBuilder.DropTable(
//                name: "AspNetRoleClaims");
//
//            migrationBuilder.DropTable(
//                name: "AspNetUserClaims");
//
//            migrationBuilder.DropTable(
//                name: "AspNetUserLogins");
//
//            migrationBuilder.DropTable(
//                name: "AspNetUserRoles");
//
//            migrationBuilder.DropTable(
//                name: "AspNetUserTokens");
//
//            migrationBuilder.DropTable(
//                name: "AspNetRoles");
//
//            migrationBuilder.DropTable(
//                name: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "CredentialTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(maxLength: 32, nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    Position = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(maxLength: 32, nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    Position = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(maxLength: 32, nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    Position = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(nullable: false),
                    PermissionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CredentialTypeId = table.Column<int>(nullable: false),
                    Extra = table.Column<string>(nullable: true),
                    Identifier = table.Column<string>(maxLength: 64, nullable: false),
                    Secret = table.Column<string>(maxLength: 1024, nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credentials_CredentialTypes_CredentialTypeId",
                        column: x => x.CredentialTypeId,
                        principalTable: "CredentialTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Credentials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_CredentialTypeId",
                table: "Credentials",
                column: "CredentialTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_UserId",
                table: "Credentials",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "CredentialTypes");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            // here
//            migrationBuilder.CreateTable(
//                name: "AspNetRoles",
//                columns: table => new
//                {
//                    Id = table.Column<string>(nullable: false),
//                    ConcurrencyStamp = table.Column<string>(nullable: true),
//                    Name = table.Column<string>(maxLength: 256, nullable: true),
//                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
//                });
//
//            migrationBuilder.CreateTable(
//                name: "AspNetUsers",
//                columns: table => new
//                {
//                    Id = table.Column<string>(nullable: false),
//                    AccessFailedCount = table.Column<int>(nullable: false),
//                    ConcurrencyStamp = table.Column<string>(nullable: true),
//                    Email = table.Column<string>(maxLength: 256, nullable: true),
//                    EmailConfirmed = table.Column<bool>(nullable: false),
//                    LockoutEnabled = table.Column<bool>(nullable: false),
//                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
//                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
//                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
//                    PasswordHash = table.Column<string>(nullable: true),
//                    PhoneNumber = table.Column<string>(nullable: true),
//                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
//                    SecurityStamp = table.Column<string>(nullable: true),
//                    TwoFactorEnabled = table.Column<bool>(nullable: false),
//                    UserName = table.Column<string>(maxLength: 256, nullable: true)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
//                });
//
//            migrationBuilder.CreateTable(
//                name: "AspNetRoleClaims",
//                columns: table => new
//                {
//                    Id = table.Column<int>(nullable: false)
//                        .Annotation("Sqlite:Autoincrement", true),
//                    ClaimType = table.Column<string>(nullable: true),
//                    ClaimValue = table.Column<string>(nullable: true),
//                    RoleId = table.Column<string>(nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
//                    table.ForeignKey(
//                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
//                        column: x => x.RoleId,
//                        principalTable: "AspNetRoles",
//                        principalColumn: "Id",
//                        onDelete: ReferentialAction.Cascade);
//                });
//
//            migrationBuilder.CreateTable(
//                name: "AspNetUserClaims",
//                columns: table => new
//                {
//                    Id = table.Column<int>(nullable: false)
//                        .Annotation("Sqlite:Autoincrement", true),
//                    ClaimType = table.Column<string>(nullable: true),
//                    ClaimValue = table.Column<string>(nullable: true),
//                    UserId = table.Column<string>(nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
//                    table.ForeignKey(
//                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
//                        column: x => x.UserId,
//                        principalTable: "AspNetUsers",
//                        principalColumn: "Id",
//                        onDelete: ReferentialAction.Cascade);
//                });
//
//            migrationBuilder.CreateTable(
//                name: "AspNetUserLogins",
//                columns: table => new
//                {
//                    LoginProvider = table.Column<string>(nullable: false),
//                    ProviderKey = table.Column<string>(nullable: false),
//                    ProviderDisplayName = table.Column<string>(nullable: true),
//                    UserId = table.Column<string>(nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
//                    table.ForeignKey(
//                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
//                        column: x => x.UserId,
//                        principalTable: "AspNetUsers",
//                        principalColumn: "Id",
//                        onDelete: ReferentialAction.Cascade);
//                });
//
//            migrationBuilder.CreateTable(
//                name: "AspNetUserRoles",
//                columns: table => new
//                {
//                    UserId = table.Column<string>(nullable: false),
//                    RoleId = table.Column<string>(nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
//                    table.ForeignKey(
//                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
//                        column: x => x.RoleId,
//                        principalTable: "AspNetRoles",
//                        principalColumn: "Id",
//                        onDelete: ReferentialAction.Cascade);
//                    table.ForeignKey(
//                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
//                        column: x => x.UserId,
//                        principalTable: "AspNetUsers",
//                        principalColumn: "Id",
//                        onDelete: ReferentialAction.Cascade);
//                });
//
//            migrationBuilder.CreateTable(
//                name: "AspNetUserTokens",
//                columns: table => new
//                {
//                    UserId = table.Column<string>(nullable: false),
//                    LoginProvider = table.Column<string>(nullable: false),
//                    Name = table.Column<string>(nullable: false),
//                    Value = table.Column<string>(nullable: true)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
//                    table.ForeignKey(
//                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
//                        column: x => x.UserId,
//                        principalTable: "AspNetUsers",
//                        principalColumn: "Id",
//                        onDelete: ReferentialAction.Cascade);
//                });
//
//            migrationBuilder.CreateIndex(
//                name: "IX_AspNetRoleClaims_RoleId",
//                table: "AspNetRoleClaims",
//                column: "RoleId");
//
//            migrationBuilder.CreateIndex(
//                name: "RoleNameIndex",
//                table: "AspNetRoles",
//                column: "NormalizedName",
//                unique: true);
//
//            migrationBuilder.CreateIndex(
//                name: "IX_AspNetUserClaims_UserId",
//                table: "AspNetUserClaims",
//                column: "UserId");
//
//            migrationBuilder.CreateIndex(
//                name: "IX_AspNetUserLogins_UserId",
//                table: "AspNetUserLogins",
//                column: "UserId");
//
//            migrationBuilder.CreateIndex(
//                name: "IX_AspNetUserRoles_RoleId",
//                table: "AspNetUserRoles",
//                column: "RoleId");
//
//            migrationBuilder.CreateIndex(
//                name: "EmailIndex",
//                table: "AspNetUsers",
//                column: "NormalizedEmail");
//
//            migrationBuilder.CreateIndex(
//                name: "UserNameIndex",
//                table: "AspNetUsers",
//                column: "NormalizedUserName",
//                unique: true);
        }
    }
}
