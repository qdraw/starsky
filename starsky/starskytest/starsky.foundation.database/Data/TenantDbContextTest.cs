using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Tenancy;

namespace starskytest.starsky.foundation.database.Data;

[TestClass]
public sealed class TenantDbContextTest
{
	public TestContext TestContext { get; set; } = null!;

	private static DbContextOptions<ApplicationDbContext> Options(SqliteConnection connection)
	{
		return new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
	}

	private static ApplicationDbContext ContextFor(SqliteConnection connection, string tenantId)
	{
		var tenant = new TenantContext();
		tenant.SetTenant(tenantId);
		return new ApplicationDbContext(Options(connection), tenant);
	}

	private async Task<SqliteConnection> CreateDatabase()
	{
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync(TestContext.CancellationToken);
		await using var context = new ApplicationDbContext(Options(connection));
		await context.Database.EnsureCreatedAsync(TestContext.CancellationToken);
		return connection;
	}

	private static FileIndexItem File(string path, string tags)
	{
		return new FileIndexItem(path) { Tags = tags };
	}

	[TestMethod]
	public void Model_EveryTenantEntity_HasTenantColumnAndQueryFilter()
	{
		using var connection = new SqliteConnection("Filename=:memory:");
		using var context = new ApplicationDbContext(Options(connection));

		var tenantTypes = typeof(ITenantEntity).Assembly.GetTypes()
			.Where(t => t is { IsClass: true } && typeof(ITenantEntity).IsAssignableFrom(t))
			.ToList();
		Assert.IsNotEmpty(tenantTypes);

		foreach ( var type in tenantTypes )
		{
			var entityType = context.Model.FindEntityType(type);
			Assert.IsNotNull(entityType, $"{type.Name} is not part of the model");

			var tenantProperty = entityType.FindProperty(ITenantEntity.TenantPropertyName);
			Assert.IsNotNull(tenantProperty, $"{type.Name} has no tenant column");
			Assert.IsTrue(tenantProperty.IsShadowProperty(), $"{type.Name} tenant is not shadow");
			Assert.IsFalse(tenantProperty.IsNullable, $"{type.Name} tenant is nullable");
			Assert.IsTrue(tenantProperty.IsConcurrencyToken, $"{type.Name} tenant is no token");
			Assert.IsNotEmpty(entityType.GetDeclaredQueryFilters().ToList(),
				$"{type.Name} has no query filter");
		}
	}

	[TestMethod]
	public void Model_EntitiesWithoutMarker_HaveNoTenantFilter()
	{
		using var connection = new SqliteConnection("Filename=:memory:");
		using var context = new ApplicationDbContext(Options(connection));

		// memberships are global rows that name a tenant: no filter, they are read across tenants
		var allowed = new[] { typeof(UserRole) };
		foreach ( var entityType in context.Model.GetEntityTypes()
			         .Where(e => !typeof(ITenantEntity).IsAssignableFrom(e.ClrType) &&
			                     !allowed.Contains(e.ClrType)) )
		{
			Assert.IsNull(entityType.FindProperty(ITenantEntity.TenantPropertyName),
				$"{entityType.ClrType.Name} has a tenant column but no ITenantEntity");
		}
	}

	[TestMethod]
	public async Task SamePath_InTwoTenants_NeverMixes()
	{
		await using var connection = await CreateDatabase();

		await using ( var acme = ContextFor(connection, "default") )
		{
			acme.FileIndex.Add(File("/same.jpg", "default-tag"));
			await acme.SaveChangesAsync(TestContext.CancellationToken);
		}

		await using ( var beta = ContextFor(connection, "beta") )
		{
			beta.FileIndex.Add(File("/same.jpg", "beta-tag"));
			await beta.SaveChangesAsync(TestContext.CancellationToken);
		}

		await using var defaultContext = ContextFor(connection, "default");
		await using var betaContext = ContextFor(connection, "beta");
		var defaultItems = await defaultContext.FileIndex.ToListAsync(TestContext.CancellationToken);
		var betaItems = await betaContext.FileIndex.ToListAsync(TestContext.CancellationToken);

		Assert.HasCount(1, defaultItems);
		Assert.AreEqual("default-tag", defaultItems[0].Tags);
		Assert.HasCount(1, betaItems);
		Assert.AreEqual("beta-tag", betaItems[0].Tags);

		// without a context for a tenant nothing is visible
		await using var other = ContextFor(connection, "gamma");
		Assert.AreEqual(0, await other.FileIndex.CountAsync(TestContext.CancellationToken));
	}

	[TestMethod]
	public async Task SameKey_InTwoTenants_ForSettingsAndThumbnails()
	{
		await using var connection = await CreateDatabase();

		foreach ( var tenant in new[] { "default", "beta" } )
		{
			await using var context = ContextFor(connection, tenant);
			context.Settings.Add(new SettingsItem { Key = "k", Value = tenant });
			context.Thumbnails.Add(new ThumbnailItem("hash", true, true, true, true, tenant));
			await context.SaveChangesAsync(TestContext.CancellationToken);
		}

		await using var beta = ContextFor(connection, "beta");
		Assert.AreEqual("beta", ( await beta.Settings.SingleAsync(TestContext.CancellationToken) ).Value);
		Assert.AreEqual("beta",
			( await beta.Thumbnails.SingleAsync(TestContext.CancellationToken) ).Reasons);
	}

	[TestMethod]
	public async Task UpdateAttachedEntity_OfAnotherTenant_Fails()
	{
		await using var connection = await CreateDatabase();
		int id;
		await using ( var owner = ContextFor(connection, "default") )
		{
			var item = File("/mine.jpg", "original");
			owner.FileIndex.Add(item);
			await owner.SaveChangesAsync(TestContext.CancellationToken);
			id = item.Id;
		}

		// a tenant that knows the id of a row it does not own
		await using ( var attacker = ContextFor(connection, "beta") )
		{
			attacker.FileIndex.Update(new FileIndexItem("/mine.jpg") { Id = id, Tags = "hacked" });
			await Assert.ThrowsExactlyAsync<DbUpdateConcurrencyException>(() =>
				attacker.SaveChangesAsync(TestContext.CancellationToken));
		}

		await using ( var attacker = ContextFor(connection, "beta") )
		{
			attacker.FileIndex.Remove(new FileIndexItem("/mine.jpg") { Id = id });
			await Assert.ThrowsExactlyAsync<DbUpdateConcurrencyException>(() =>
				attacker.SaveChangesAsync(TestContext.CancellationToken));
		}

		await using var verify = ContextFor(connection, "default");
		Assert.AreEqual("original",
			( await verify.FileIndex.SingleAsync(TestContext.CancellationToken) ).Tags);
	}

	[TestMethod]
	public async Task UpdateAttachedEntity_OfOwnTenant_Works()
	{
		await using var connection = await CreateDatabase();
		int id;
		await using ( var context = ContextFor(connection, "beta") )
		{
			var item = File("/mine.jpg", "original");
			context.FileIndex.Add(item);
			await context.SaveChangesAsync(TestContext.CancellationToken);
			id = item.Id;
		}

		await using ( var context = ContextFor(connection, "beta") )
		{
			context.FileIndex.Update(new FileIndexItem("/mine.jpg") { Id = id, Tags = "changed" });
			await context.SaveChangesAsync(TestContext.CancellationToken);
		}

		await using var verify = ContextFor(connection, "beta");
		Assert.AreEqual("changed",
			( await verify.FileIndex.SingleAsync(TestContext.CancellationToken) ).Tags);
	}

	[TestMethod]
	public async Task ChangingTenantOfExistingRow_Throws()
	{
		await using var connection = await CreateDatabase();
		await using var context = ContextFor(connection, "default");
		var item = File("/a.jpg", "a");
		context.FileIndex.Add(item);
		await context.SaveChangesAsync(TestContext.CancellationToken);

		context.Entry(item).Property(ITenantEntity.TenantPropertyName).CurrentValue = "beta";

		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
			context.SaveChangesAsync(TestContext.CancellationToken));
	}

	[TestMethod]
	public async Task AddingEntityOfAnotherTenant_Throws()
	{
		await using var connection = await CreateDatabase();
		await using var context = ContextFor(connection, "default");
		var entry = context.FileIndex.Add(File("/a.jpg", "a"));
		entry.Property(ITenantEntity.TenantPropertyName).CurrentValue = "beta";

		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
			context.SaveChangesAsync(TestContext.CancellationToken));
	}

	[TestMethod]
	public async Task ExecuteDelete_OnlyAffectsOwnTenant()
	{
		await using var connection = await CreateDatabase();
		foreach ( var tenant in new[] { "default", "beta" } )
		{
			await using var context = ContextFor(connection, tenant);
			context.Notifications.Add(new NotificationItem
			{
				Content = tenant, DateTime = DateTime.UtcNow, DateTimeEpoch = 1
			});
			await context.SaveChangesAsync(TestContext.CancellationToken);
		}

		await using ( var beta = ContextFor(connection, "beta") )
		{
			await beta.Notifications.ExecuteDeleteAsync(TestContext.CancellationToken);
		}

		await using var verify = ContextFor(connection, "default");
		Assert.AreEqual("default",
			( await verify.Notifications.SingleAsync(TestContext.CancellationToken) ).Content);
	}

	[TestMethod]
	public async Task Migration_ExistingRows_BecomeDefaultTenant()
	{
		await using var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync(TestContext.CancellationToken);
		await using ( var context = new ApplicationDbContext(Options(connection)) )
		{
			var migrator = context.GetService<IMigrator>();
			await migrator.MigrateAsync("20260505142535_QueueItemsTableAddCustomAiTags",
				TestContext.CancellationToken);
			await context.Database.ExecuteSqlRawAsync(
				"INSERT INTO Settings (Key, Value, IsUserEditable, UserId) VALUES ('old','v',0,-1)",
				TestContext.CancellationToken);
			await migrator.MigrateAsync(null, TestContext.CancellationToken);
		}

		await using var verify = ContextFor(connection, "default");
		Assert.AreEqual("v",
			( await verify.Settings.SingleAsync(s => s.Key == "old", TestContext.CancellationToken) )
			.Value);
		await using var other = ContextFor(connection, "beta");
		Assert.AreEqual(0, await other.Settings.CountAsync(TestContext.CancellationToken));
	}
}
