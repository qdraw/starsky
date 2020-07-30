﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using starsky.foundation.database.Data;

namespace starsky.foundation.database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5");

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
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<string>("Secret")
                        .HasColumnType("TEXT")
                        .HasMaxLength(1024);

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CredentialTypeId");

                    b.HasIndex("UserId");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.CredentialType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(32);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<int?>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("CredentialTypes");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(32);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<int?>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(32);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<int?>("Position")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.RolePermission", b =>
                {
                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PermissionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RoleId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("RolePermissions");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.Account.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
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
                        .HasColumnType("TEXT")
                        .HasMaxLength(190);

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(190);

                    b.Property<string>("FilePath")
                        .HasColumnType("TEXT")
                        .HasMaxLength(380);

                    b.Property<double>("FocalLength")
                        .HasColumnType("REAL");

                    b.Property<int>("ImageFormat")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("ImageHeight")
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
                        .HasColumnType("TEXT")
                        .HasMaxLength(40);

                    b.Property<string>("LocationCountry")
                        .HasColumnType("TEXT")
                        .HasMaxLength(40);

                    b.Property<string>("LocationState")
                        .HasColumnType("TEXT")
                        .HasMaxLength(40);

                    b.Property<double>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<string>("MakeModel")
                        .HasColumnType("TEXT");

                    b.Property<int>("Orientation")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ParentDirectory")
                        .HasColumnType("TEXT")
                        .HasMaxLength(190);

                    b.Property<string>("ShutterSpeed")
                        .HasColumnType("TEXT")
                        .HasMaxLength(20);

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Software")
                        .HasColumnType("TEXT")
                        .HasMaxLength(40);

                    b.Property<string>("Tags")
                        .HasColumnType("TEXT")
                        .HasMaxLength(1024);

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileName", "ParentDirectory");

                    b.ToTable("FileIndex");
                });

            modelBuilder.Entity("starsky.foundation.database.Models.ImportIndexItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddToDatabase")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("FilePath")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ImportIndex");
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
                });
#pragma warning restore 612, 618
        }
    }
}
