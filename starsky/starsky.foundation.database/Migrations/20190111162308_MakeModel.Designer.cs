﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using starsky.foundation.database.Data;

namespace starsky.foundation.database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20190111162308_MakeModel")]
    partial class MakeModel
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity("starsky.Models.Account.Credential", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CredentialTypeId");

                    b.Property<string>("Extra");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<string>("Secret")
                        .HasMaxLength(1024);

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("CredentialTypeId");

                    b.HasIndex("UserId");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("starsky.Models.Account.CredentialType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<int?>("Position");

                    b.HasKey("Id");

                    b.ToTable("CredentialTypes");
                });

            modelBuilder.Entity("starsky.Models.Account.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<int?>("Position");

                    b.HasKey("Id");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("starsky.Models.Account.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<int?>("Position");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("starsky.Models.Account.RolePermission", b =>
                {
                    b.Property<int>("RoleId");

                    b.Property<int>("PermissionId");

                    b.HasKey("RoleId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("RolePermissions");
                });

            modelBuilder.Entity("starsky.Models.Account.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("starsky.Models.Account.UserRole", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<int>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("starsky.Models.FileIndexItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AddToDatabase");

                    b.Property<double>("Aperture");

                    b.Property<int>("ColorClass");

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("Description");

                    b.Property<string>("FileHash");

                    b.Property<string>("FileName");

                    b.Property<string>("FilePath");

                    b.Property<int>("ImageFormat");

                    b.Property<ushort>("ImageHeight");

                    b.Property<ushort>("ImageWidth");

                    b.Property<bool>("IsDirectory");

                    b.Property<ushort>("IsoSpeed");

                    b.Property<double>("Latitude");

                    b.Property<double>("LocationAltitude");

                    b.Property<string>("LocationCity")
                        .HasMaxLength(40);

                    b.Property<string>("LocationCountry")
                        .HasMaxLength(40);

                    b.Property<string>("LocationState")
                        .HasMaxLength(40);

                    b.Property<double>("Longitude");

                    b.Property<string>("MakeModel");

                    b.Property<int>("Orientation");

                    b.Property<string>("ParentDirectory");

                    b.Property<string>("ShutterSpeed")
                        .HasMaxLength(20);

                    b.Property<string>("Tags");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("FileIndex");
                });

            modelBuilder.Entity("starsky.Models.ImportIndexItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AddToDatabase");

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("FileHash");

                    b.HasKey("Id");

                    b.ToTable("ImportIndex");
                });

            modelBuilder.Entity("starsky.Models.Account.Credential", b =>
                {
                    b.HasOne("starsky.Models.Account.CredentialType", "CredentialType")
                        .WithMany("Credentials")
                        .HasForeignKey("CredentialTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("starsky.Models.Account.User", "User")
                        .WithMany("Credentials")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("starsky.Models.Account.RolePermission", b =>
                {
                    b.HasOne("starsky.Models.Account.Permission", "Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("starsky.Models.Account.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("starsky.Models.Account.UserRole", b =>
                {
                    b.HasOne("starsky.Models.Account.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("starsky.Models.Account.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
