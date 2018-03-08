// ﻿using System.Data.Entity;

using Microsoft.EntityFrameworkCore;
using starsky.Models;

namespace starsky.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<FileIndexItem> FileIndex { get; set; }

        //public DbSet<SqlBotDataEntities> SqlBotDataEntities { get; set; }
        //public DbSet<HappinessStats> HappinessStats { get; set; }

        //public DbSet<FCT_Stats> FCT_Stats { get; set; }

    }
}
