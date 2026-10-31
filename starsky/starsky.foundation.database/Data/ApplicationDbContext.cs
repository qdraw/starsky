using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using starsky.foundation.database.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Tenancy;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace starsky.foundation.database.Data;

public class ApplicationDbContext : DbContext
{
	private readonly ITenantContext? _tenantContext;

	/// <param name="options">options</param>
	/// <param name="tenantContext">
	///     the tenant of the current request; null (CLI, tests, design time) is the default tenant
	/// </param>
	public ApplicationDbContext(DbContextOptions options, ITenantContext? tenantContext = null) :
		base(options)
	{
		_tenantContext = tenantContext;
		try
		{
			HookTenantTracking();
		}
		catch ( InvalidOperationException )
		{
			// no database provider configured (design time or a test double): nothing can be
			// tracked, so there is nothing to hook
		}
	}

	/// <summary>
	///     Read by the query filters on every query (EF parameterizes it per context instance)
	/// </summary>
	/// <summary>
	///     Without a tenant context (a context created outside DI, e.g. by QueryFactory in a
	///     job) the tenant of the current flow: never silently another tenant
	/// </summary>
	public string TenantId => _tenantContext?.TenantId ?? TenantAmbient.Current;

	/// <summary>
	///     Only for system services that serve all tenants (the queue backends read the jobs of
	///     every tenant). Request code never calls this: the query filters stay on by default
	/// </summary>
	public bool CrossTenantAccess { get; private set; }

	public void EnableCrossTenantAccess()
	{
		CrossTenantAccess = true;
	}

	/// <summary>
	///     Cross tenant access for the duration of one operation only (a purge of a tenant), then
	///     back to what it was
	/// </summary>
	public async Task<T> RunCrossTenantAsync<T>(Func<Task<T>> operation)
	{
		var before = CrossTenantAccess;
		CrossTenantAccess = true;
		try
		{
			return await operation();
		}
		finally
		{
			CrossTenantAccess = before;
		}
	}

	private void HookTenantTracking()
	{
		// the tenant is (part of) a key: it must be there when EF starts tracking, not at save
		ChangeTracker.Tracking += (_, args) =>
		{
			if ( args.Entry.Entity is not ITenantEntity )
			{
				return;
			}

			var tenant = args.Entry.Property(ITenantEntity.TenantPropertyName);
			if ( tenant.CurrentValue == null )
			{
				tenant.CurrentValue = TenantId;
			}
		};
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

	public DbSet<TenantItem> Tenants { get; set; }

	public DbSet<NotificationItem> Notifications { get; set; }

	public DbSet<SettingsItem> Settings { get; set; }

	public DbSet<ThumbnailItem> Thumbnails { get; set; }

	public DbSet<DiagnosticsItem> Diagnostics { get; set; }
	public DbSet<QueueItem> QueueItems { get; set; }

	/// <summary>
	///     Store secure keys to generate cookies
	/// </summary>
	public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

	public DbSet<GeoNameCity> GeoNameCities { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		// Do nothing because of that in debug mode this only triggered
#if DEBUG
		optionsBuilder.EnableSensitiveDataLogging();
#endif
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		ConfigureTenantEntities(modelBuilder);

		const string utf8Mb4 = "utf8mb4";
		const string mySqlCharSetAnnotation = "MySql:CharSet";
		const string mySqlValueGeneratedOnAdd = "MySql:ValueGeneratedOnAdd";
		const string sqliteAutoincrement = "Sqlite:Autoincrement";
		const string mysqlValuegenerationstrategy = "MySql:ValueGenerationStrategy";

		// does not have direct effect
		modelBuilder.HasCharSet(utf8Mb4,
			DelegationModes.ApplyToAll);

		// Add Index to speed performance (on MySQL max key length is 3072 bytes)
		// MySql:CharSet might be working a future release, but now it does nothing
		modelBuilder.Entity<FileIndexItem>(etb =>
		{
			etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);

			// This filters on ParentDirectory and sort on fileName
			etb.HasIndex(x => new { x.FileName, x.ParentDirectory });
			etb.HasIndex(x => new { x.ParentDirectory, x.FileName });
			etb.HasIndex(x => new { x.FilePath });

			// every query is filtered by tenant first
			etb.HasIndex(ITenantEntity.TenantPropertyName, nameof(FileIndexItem.FilePath));
			etb.HasIndex(ITenantEntity.TenantPropertyName,
				nameof(FileIndexItem.ParentDirectory), nameof(FileIndexItem.FileName));
			// The Tags Index is truncated to fit within the index size limit
			etb.HasIndex(x => new { x.Tags });
			etb.HasIndex(x => new { x.ImageFormat });

			// Search indexes - for WideSearch (SearchService.cs) performance
			// Date range search indexes
			etb.HasIndex(x => new { x.DateTime });

			// ParentDirectory index only (Tags is varchar(1024) = 4096 bytes, exceeds 3072 limit)
			etb.HasIndex(x => new { x.ParentDirectory })
				.HasDatabaseName("IX_FileIndex_ParentDirectory");

			// FileHash index improvement - ensure it supports null values
			etb.HasIndex(x => new { x.FileHash })
				.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);

			// set type for file size in bites
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
				etb.Property(e => e.LastTenantId).HasMaxLength(63);
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
				// a membership: the same user can have a role in every tenant
				etb.HasKey(e => new { e.UserId, e.TenantId, e.RoleId });
				etb.Property(e => e.TenantId).IsRequired().HasMaxLength(63)
					.HasColumnType("varchar(63)")
					.HasDefaultValue(TenantIdHelper.DefaultTenantId);
				etb.HasIndex(e => e.TenantId);
				// one role per user per tenant: two requests can not both add a membership
				etb.HasIndex(e => new { e.UserId, e.TenantId }).IsUnique()
					.HasDatabaseName("IX_UserRoles_UserId_TenantId");
				etb.ToTable("UserRoles");
			}
		);

		modelBuilder.Entity<TenantItem>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id).IsRequired().HasMaxLength(63).HasColumnType("varchar(63)");
				etb.Property(e => e.DisplayName).IsRequired().HasMaxLength(150);
				etb.Property(e => e.StorageFolder).HasMaxLength(1024);
				etb.Property(e => e.ThumbnailTempFolder).HasMaxLength(1024);
				etb.Property(e => e.TempFolder).HasMaxLength(1024);
				etb.ToTable("Tenants");
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
					.HasAnnotation(mySqlValueGeneratedOnAdd, true)
					.HasAnnotation(sqliteAutoincrement, true)
					.HasAnnotation(mysqlValuegenerationstrategy,
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
				// the same key can exist in every tenant
				etb.HasKey(ITenantEntity.TenantPropertyName, nameof(SettingsItem.Key));

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

		modelBuilder.Entity<QueueItem>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation(mySqlValueGeneratedOnAdd, true);

				etb.Property(e => e.QueueName).IsRequired().HasMaxLength(64);
				etb.Property(e => e.JobType).IsRequired().HasMaxLength(150);
				etb.Property(e => e.MetaData).HasMaxLength(1024);
				etb.Property(e => e.TraceParentId).HasMaxLength(512);
				etb.Property(e => e.Status).IsConcurrencyToken();
				etb.Property(e => e.CreatedAtUtc).HasMaxLength(27);
				etb.Property(e => e.ClaimedAtUtc).HasMaxLength(27);
				etb.Property(e => e.ProcessedAtUtc).HasMaxLength(27);
				etb.Property(e => e.JobId).HasMaxLength(36);

				etb.HasIndex(e => e.JobId)
					.HasDatabaseName("IX_QueueItems_JobId")
					.IsUnique();

				etb.ToTable("QueueItems");
			}
		);

		modelBuilder.Entity<DataProtectionKey>(etb =>
			{
				etb.HasAnnotation("MySql:CharSet", "utf8mb4");
				etb.HasKey(e => e.Id);
				etb.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.HasAnnotation(mySqlValueGeneratedOnAdd, true);

				etb.Property(e => e.Xml).HasMaxLength(1200);
				etb.Property(e => e.FriendlyName).HasMaxLength(45);
			}
		);

		modelBuilder.Entity<ThumbnailItem>(etb =>
			{
				etb.Property(e => e.FileHash).IsRequired().HasMaxLength(190);
				// the same file content (hash) can exist in every tenant
				etb.HasKey(ITenantEntity.TenantPropertyName, nameof(ThumbnailItem.FileHash));

				etb.ToTable("Thumbnails");
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);

				// Add composite index for performance on GetMissingThumbnailsBatchInternalAsync
				// FileHash is excluded as it's already the primary key
				// This keeps the index size under MariaDB's 3072 byte limit
				etb.HasIndex(e => new { e.ExtraLarge, e.Large, e.Small })
					.HasDatabaseName("IX_Thumbnails_Missing");
			}
		);

		modelBuilder.Entity<GeoNameCity>(etb =>
			{
				etb.HasAnnotation(mySqlCharSetAnnotation, utf8Mb4);
				etb.ToTable("GeoNameCities");

				etb.Property(e => e.GeonameId)
					.ValueGeneratedNever()
					.HasAnnotation(mySqlValueGeneratedOnAdd, false)
					.HasAnnotation(sqliteAutoincrement, false)
					.HasAnnotation(mysqlValuegenerationstrategy,
						MySqlValueGenerationStrategy.IdentityColumn);
			}
		);
	}

	private void ConfigureTenantEntities(ModelBuilder modelBuilder)
	{
		foreach ( var clrType in modelBuilder.Model.GetEntityTypes()
			         .Select(e => e.ClrType)
			         .Where(t => typeof(ITenantEntity).IsAssignableFrom(t)).ToList() )
		{
			var entity = modelBuilder.Entity(clrType);
			entity.Property<string>(ITenantEntity.TenantPropertyName)
				// a key column: TEXT can not be (part of) a key in MySQL
				.HasColumnType("varchar(63)")
				.HasMaxLength(63)
				.IsRequired()
				// existing rows become the default tenant
				.HasDefaultValue(TenantIdHelper.DefaultTenantId)
				// as part of a string key EF would generate a guid: the value comes from the context
				.ValueGeneratedNever()
				// an update or delete that targets a row of another tenant matches 0 rows and fails
				.IsConcurrencyToken();

			entity.HasQueryFilter(CreateTenantFilter(clrType));
		}
	}

	/// <summary>
	///     entity => CrossTenantAccess || EF.Property(entity, "TenantId") == TenantId, where
	///     CrossTenantAccess and TenantId are read from this context instance on every query
	/// </summary>
	private LambdaExpression CreateTenantFilter(Type clrType)
	{
		var entity = Expression.Parameter(clrType, "entity");
		var tenantOfRow = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(string)],
			entity, Expression.Constant(ITenantEntity.TenantPropertyName));
		var context = Expression.Constant(this);
		var isOfThisTenant = Expression.Equal(tenantOfRow,
			Expression.Property(context, nameof(TenantId)));
		return Expression.Lambda(
			Expression.OrElse(Expression.Property(context, nameof(CrossTenantAccess)),
				isOfThisTenant), entity);
	}

	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		EnforceTenant();
		return base.SaveChanges(acceptAllChangesOnSuccess);
	}

	public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
		CancellationToken cancellationToken = default)
	{
		EnforceTenant();
		return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	/// <summary>
	///     New rows get the tenant of this context. Existing rows must already belong to it and
	///     can never change tenant. Entities that were not loaded (Update/Attach with a key) have
	///     no tenant yet: they get the context's tenant as the original value, so the update only
	///     matches a row of this tenant
	/// </summary>
	private void EnforceTenant()
	{
		foreach ( var entry in ChangeTracker.Entries().Where(e => e.Entity is ITenantEntity) )
		{
			var tenant = entry.Property(ITenantEntity.TenantPropertyName);
			switch ( entry.State )
			{
				case EntityState.Added:
					EnforceTenantOfNewEntity(tenant);
					break;
				case EntityState.Modified or EntityState.Deleted or EntityState.Unchanged:
					EnforceTenantOfExistingEntity(tenant);
					break;
			}
		}
	}

	private void EnforceTenantOfNewEntity(PropertyEntry tenant)
	{
		// cross tenant access adds for the tenant that is set on the entity
		if ( CrossTenantAccess && tenant.CurrentValue is string explicitTenant &&
		     TenantIdHelper.IsValid(explicitTenant) )
		{
			return;
		}

		if ( tenant.CurrentValue is string added && added != TenantId )
		{
			throw new InvalidOperationException("An entity of another tenant can not be added");
		}

		tenant.CurrentValue = TenantId;
	}

	private void EnforceTenantOfExistingEntity(PropertyEntry tenant)
	{
		if ( tenant.CurrentValue == null && !CrossTenantAccess )
		{
			tenant.CurrentValue = TenantId;
			tenant.OriginalValue = TenantId;
			tenant.IsModified = false;
			return;
		}

		// never a change of tenant; without cross tenant access also only own rows
		if ( tenant.CurrentValue == null ||
		     !tenant.CurrentValue.Equals(tenant.OriginalValue) ||
		     ( !CrossTenantAccess && !TenantId.Equals(tenant.CurrentValue) ) )
		{
			throw new InvalidOperationException(
				"An entity of another tenant can not be changed or deleted");
		}
	}
}
