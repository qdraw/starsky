// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using starsky.Models;
using starsky.Models.Account;

namespace starsky.Data
{
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<FileIndexItem> FileIndex { get; set; }
		public DbSet<ImportIndexItem> ImportIndex { get; set; }

		public DbSet<User> Users { get; set; }
		public DbSet<CredentialType> CredentialTypes { get; set; }
		public DbSet<Credential> Credentials { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<Permission> Permissions { get; set; }
		public DbSet<RolePermission> RolePermissions { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			#if (DEBUG) 
				optionsBuilder.EnableSensitiveDataLogging();
			#endif
		}
			
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>(etb =>
				{
					etb.HasKey(e => e.Id);
					etb.Property(e => e.Id).ValueGeneratedOnAdd();
					etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
					etb.ToTable("Users");
				}
			);

			modelBuilder.Entity<CredentialType>(etb =>
				{
					etb.HasKey(e => e.Id);
					etb.Property(e => e.Id).ValueGeneratedOnAdd();
					etb.Property(e => e.Code).IsRequired().HasMaxLength(32);
					etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
					etb.ToTable("CredentialTypes");
				}
			);

			modelBuilder.Entity<Credential>(etb =>
				{
					etb.HasKey(e => e.Id);
					etb.Property(e => e.Id).ValueGeneratedOnAdd();
					etb.Property(e => e.Identifier).IsRequired().HasMaxLength(64);
					etb.Property(e => e.Secret).HasMaxLength(1024);
					etb.ToTable("Credentials");
				}
			);

			modelBuilder.Entity<Role>(etb =>
				{
					etb.HasKey(e => e.Id);
					etb.Property(e => e.Id).ValueGeneratedOnAdd();
					etb.Property(e => e.Code).IsRequired().HasMaxLength(32);
					etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
					etb.ToTable("Roles");
				}
			);

			modelBuilder.Entity<UserRole>(etb =>
				{
					etb.HasKey(e => new { e.UserId, e.RoleId });
					etb.ToTable("UserRoles");
				}
			);

			modelBuilder.Entity<Permission>(etb =>
				{
					etb.HasKey(e => e.Id);
					etb.Property(e => e.Id).ValueGeneratedOnAdd();
					etb.Property(e => e.Code).IsRequired().HasMaxLength(32);
					etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
					etb.ToTable("Permissions");
				}
			);

			modelBuilder.Entity<RolePermission>(etb =>
				{
					etb.HasKey(e => new { e.RoleId, e.PermissionId });
					etb.ToTable("RolePermissions");
				}
			);
		}
	}
}