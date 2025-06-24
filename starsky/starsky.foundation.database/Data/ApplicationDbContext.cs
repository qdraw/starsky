using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using starsky.foundation.database.Models;
using starsky.foundation.database.Models.Account;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace starsky.foundation.database.Data;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions options) : base(options)
	{
	}

	public virtual DbSet<FileIndexItem> FileIndex { get; set; }
	public DbSet<ImportIndexItem> ImportIndex { get; set; }

	public virtual DbSet<User> Users { get; set; }
	public DbSet<CredentialType> CredentialTypes { get; set; }
	public DbSet<Credential> Credentials { get; set; }
	public DbSet<Role> Roles { get; set; }
	public DbSet<UserRole> UserRoles { get; set; }
	public DbSet<Permission> Permissions { get; set; }
	public DbSet<RolePermission> RolePermissions { get; set; }

	public DbSet<NotificationItem> Notifications { get; set; }

	public DbSet<SettingsItem> Settings { get; set; }

	public DbSet<ThumbnailItem> Thumbnails { get; set; }

	public DbSet<DiagnosticsItem> Diagnostics { get; set; }

	/// <summary>
	///     Store secure keys to generate cookies
	/// </summary>
	public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		// Do nothing because of that in debug mode this only triggered
#if ( DEBUG )
		optionsBuilder.EnableSensitiveDataLogging();
#endif
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		const string utf8Mb4 = "utf8mb4";
		const string mySqlCharSetAnnotation = "MySql:CharSet";
		const string mySqlValueGeneratedOnAdd = "MySql:ValueGeneratedOnAdd";

		// does not have direct effect
		modelBuilder.HasCharSet(utf8Mb4,
			DelegationModes.ApplyToAll);

		// Add Index to speed performance (on MySQL max key length is 3072 bytes)
		// MySql:CharSet might be working a future release but now it does nothing
		modelBuilder.Entity<FileIndexItem>(etb =>
		{
			etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
			etb.HasIndex(x => new { x.FileName, x.ParentDirectory });
			// This filters on ParentDirectory and sort on fileName
			etb.HasIndex(x => new { x.ParentDirectory, x.FileName });

			etb.HasIndex(x => new { x.FilePath });
			etb.HasIndex(x => new { x.Tags });
			etb.HasIndex(x => new { x.ImageFormat });

			etb.Property(p => p.Size).HasColumnType("bigint");
		});

		modelBuilder.Entity<User>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation(mySqlValueGeneratedOnAdd, true);
				etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
				etb.ToTable("Users");

				DateTime parsedDateTime;
				var converter = new ValueConverter<DateTime, string>(
					v =>
						v.ToString(@"yyyy\-MM\-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
					v => DateTime.TryParseExact(v, @"yyyy\-MM\-dd HH:mm:ss.fff",
						CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
						out parsedDateTime)
						? parsedDateTime
						: DateTime.MinValue
				);

				etb.Property(e => e.LockoutEnd)
					.HasColumnType("TEXT")
					.HasConversion(converter);
			}
		);

		modelBuilder.Entity<CredentialType>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation(mySqlValueGeneratedOnAdd, true);
				etb.Property(e => e.Code).IsRequired().HasMaxLength(32);
				etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
				etb.ToTable("CredentialTypes");
			}
		);

		modelBuilder.Entity<Credential>(etb =>
		{
			etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
			etb.HasKey(e => e.Id);
			etb.Property(e => e.Id)
				.ValueGeneratedOnAdd()
				.HasAnnotation(mySqlValueGeneratedOnAdd, true);
			etb.Property(e => e.Identifier).IsRequired().HasMaxLength(64);
			etb.Property(e => e.Secret).HasMaxLength(1024);
			etb.ToTable("Credentials");
		});

		modelBuilder.Entity<Credential>()
			.HasIndex(x => new { x.Id, x.Identifier });

		modelBuilder.Entity<Role>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation(mySqlValueGeneratedOnAdd, true);
				etb.Property(e => e.Code).IsRequired().HasMaxLength(32);
				etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
				etb.ToTable("Roles");
			}
		);

		modelBuilder.Entity<UserRole>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => new { e.UserId, e.RoleId });
				etb.ToTable("UserRoles");
			}
		);

		modelBuilder.Entity<Permission>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation(mySqlValueGeneratedOnAdd, true);
				etb.Property(e => e.Code).IsRequired().HasMaxLength(32);
				etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
				etb.ToTable("Permissions");
			}
		);

		modelBuilder.Entity<RolePermission>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => new { e.RoleId, e.PermissionId });
				etb.ToTable("RolePermissions");
			}
		);

		modelBuilder.Entity<ImportIndexItem>()
			.HasIndex(x => new { x.FileHash })
			.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);

		modelBuilder.Entity<NotificationItem>(etb =>
			{
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation("MySql:ValueGeneratedOnAdd", true)
					.HasAnnotation("Sqlite:Autoincrement", true)
					.HasAnnotation("MySql:ValueGenerationStrategy",
						MySqlValueGenerationStrategy.IdentityColumn);

				etb.Property(p => p.Content)
					.HasColumnType("mediumtext");

				etb.Property(p => p.DateTime)
					.IsConcurrencyToken();

				etb.Property(p => p.DateTimeEpoch).HasColumnType("bigint");

				etb.HasIndex(x => new { x.DateTimeEpoch });

				etb.ToTable("Notifications");
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
			}
		);

		modelBuilder.Entity<SettingsItem>(etb =>
			{
				etb.Property(e => e.Key).IsRequired().HasMaxLength(150);
				etb.HasKey(e => e.Key);

				etb.ToTable("Settings");
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
			}
		);

		modelBuilder.Entity<DiagnosticsItem>(etb =>
			{
				etb.Property(e => e.Key).IsRequired().HasMaxLength(150);
				etb.HasKey(e => e.Key);

				etb.ToTable("Diagnostics");
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
			}
		);

		modelBuilder.Entity<DataProtectionKey>(etb =>
			{
				etb.HasAnnotation("MySql:CharSet", "utf8mb4");
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation("MySql:ValueGeneratedOnAdd", true);

				etb.Property(e => e.Xml).HasMaxLength(1200);
				etb.Property(e => e.FriendlyName).HasMaxLength(45);
			}
		);

		modelBuilder.Entity<ThumbnailItem>(etb =>
			{
				etb.Property(e => e.FileHash).IsRequired().HasMaxLength(190);
				etb.HasKey(e => e.FileHash);

				etb.ToTable("Thumbnails");
				etb.HasAnnotation("MySql:CharSet", "utf8mb4");
			}
		);
	}
}
