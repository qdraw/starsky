﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using starsky.Data;
using starsky.Models;
using System;

namespace starsky.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180322104602_ColorClassFeature")]
    partial class ColorClassFeature
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("starsky.Models.FileIndexItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AddToDatabase");

                    b.Property<int>("ColorClass");

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("Description");

                    b.Property<string>("FileHash");

                    b.Property<string>("FileName");

                    b.Property<string>("FilePath");

                    b.Property<bool>("IsDirectory");

                    b.Property<string>("ParentDirectory");

                    b.Property<string>("Tags");

                    b.HasKey("Id");

                    b.ToTable("FileIndex");
                });
#pragma warning restore 612, 618
        }
    }
}
