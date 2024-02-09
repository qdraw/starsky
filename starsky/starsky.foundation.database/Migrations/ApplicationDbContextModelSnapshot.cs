﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using starsky.foundation.database.Data;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("MySql:CharSet", "utf8mb4")
                .HasAnnotation("MySql:CharSetDelegation", DelegationModes.ApplyToAll)
                .HasAnnotation("ProductVersion", "8.0.1");

            modelBuilder.Entity("starsky.foundation.database.Models.Account.Credential", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<int>("CredentialTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Extra")
                        .HasColumnType("TEXT");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<string>("Secret")
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CredentialTypeId");

                    b.HasIndex("UserId");

                    b.HasIndex("Id", "Identifier");

                    b.ToTable("Credentials", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.CredentialType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("CredentialTypes", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Permissions", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Roles", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.RolePermission", b =>
                {
                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PermissionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RoleId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("RolePermissions", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LockoutEnd")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("FriendlyName")
                        .HasMaxLength(45)
                        .HasColumnType("TEXT");

                    b.Property<string>("Xml")
                        .HasMaxLength(1200)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.FileIndexItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddToDatabase")
                        .HasColumnType("TEXT");

                    b.Property<double>("Aperture")
                        .HasColumnType("REAL");

                    b.Property<int>("ColorClass")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileHash")
                        .HasMaxLength(190)
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .HasMaxLength(190)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(1);

                    b.Property<string>("FilePath")
                        .HasMaxLength(380)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(2);

                    b.Property<double>("FocalLength")
                        .HasColumnType("REAL");

                    b.Property<int>("ImageFormat")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("ImageHeight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ImageStabilisation")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("ImageWidth")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("IsDirectory")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("IsoSpeed")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastEdited")
                        .HasColumnType("TEXT");

                    b.Property<double>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double>("LocationAltitude")
                        .HasColumnType("REAL");

                    b.Property<string>("LocationCity")
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("LocationCountry")
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("LocationCountryCode")
                        .HasMaxLength(3)
                        .HasColumnType("TEXT");

                    b.Property<string>("LocationState")
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<double>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<string>("MakeModel")
                        .HasColumnType("TEXT");

                    b.Property<int>("Orientation")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ParentDirectory")
                        .HasMaxLength(190)
                        .HasColumnType("TEXT");

                    b.Property<string>("ShutterSpeed")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("SidecarExtensions")
                        .HasColumnType("TEXT");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<string>("Software")
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .HasMaxLength(1024)
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileName", "ParentDirectory");

                    b.ToTable("FileIndex");

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.ImportIndexItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddToDatabase")
                        .HasColumnType("TEXT");

                    b.Property<int>("ColorClass")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("DateTimeFromFileName")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("FilePath")
                        .HasColumnType("TEXT");

                    b.Property<string>("MakeModel")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileHash")
                        .HasAnnotation("MySql:CharSet", "utf8mb4");

                    b.ToTable("ImportIndex");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.NotificationItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true)
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                        .HasAnnotation("Sqlite:Autoincrement", true);

                    b.Property<string>("Content")
                        .HasColumnType("mediumtext");

                    b.Property<DateTime>("DateTime")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<long>("DateTimeEpoch")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Notifications", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.SettingsItem", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(150)
                        .HasColumnType("varchar(150)");

                    b.Property<bool>("IsUserEditable")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(4096)
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("Settings", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.ThumbnailItem", b =>
                {
                    b.Property<string>("FileHash")
                        .HasMaxLength(190)
                        .HasColumnType("varchar(190)");

                    b.Property<bool?>("ExtraLarge")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("Large")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reasons")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("Small")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("TinyMeta")
                        .HasColumnType("INTEGER");

                    b.HasKey("FileHash");

                    b.ToTable("Thumbnails", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8mb4");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.Credential", b =>
                {
                    b.HasOne("starsky.foundation.database.Models.Account.CredentialType", "CredentialType")
                        .WithMany("Credentials")
                        .HasForeignKey("CredentialTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("starsky.foundation.database.Models.Account.User", "User")
                        .WithMany("Credentials")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CredentialType");

                    b.Navigation("User");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.RolePermission", b =>
                {
                    b.HasOne("starsky.foundation.database.Models.Account.Permission", "Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("starsky.foundation.database.Models.Account.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.UserRole", b =>
                {
                    b.HasOne("starsky.foundation.database.Models.Account.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("starsky.foundation.database.Models.Account.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.CredentialType", b =>
                {
                    b.Navigation("Credentials");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.User", b =>
                {
                    b.Navigation("Credentials");
                });
#pragma warning restore 612, 618
        }
    }
}
