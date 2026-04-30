using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Interfaces;

namespace starskytest.starsky.foundation.database.Data;

[TestClass]
public sealed class ApplicationDbContextTenantIsolationTest : IDisposable
{
	private readonly SqliteConnection _connection;
	public TestContext TestContext { get; set; }

	public ApplicationDbContextTenantIsolationTest()
	{
		_connection = new SqliteConnection("Filename=:memory:");
		_connection.Open();

		using var setupContext = CreateContext();
		setupContext.Database.EnsureCreated();
	}

	[TestMethod]
	public async Task FileIndex_Query_AsTenantA_ReturnsZeroItemsFromTenantB()
	{
		int tenantAId;
		string tenantASlug;

		var cancellationToken = TestContext.CancellationTokenSource.Token;

		await using ( var seedContext = CreateContext() )
		{
			var tenantA = new Tenant
			{
				Slug = "tenant-a",
				Name = "Tenant A",
				IsEnabled = true,
				Created = DateTime.UtcNow
			};
			var tenantB = new Tenant
			{
				Slug = "tenant-b",
				Name = "Tenant B",
				IsEnabled = true,
				Created = DateTime.UtcNow
			};

			await seedContext.Tenants.AddRangeAsync(tenantA, tenantB);
			await seedContext.SaveChangesAsync(cancellationToken);

			tenantAId = tenantA.Id;
			tenantASlug = tenantA.Slug;

			await seedContext.FileIndex.AddRangeAsync(
				new FileIndexItem("/shared/tenant-a-only.jpg")
				{
					TenantId = tenantA.Id,
					FileHash = "hash-tenant-a",
					Status = FileIndexItem.ExifStatus.Ok
				},
				new FileIndexItem("/shared/tenant-b-only.jpg")
				{
					TenantId = tenantB.Id,
					FileHash = "hash-tenant-b",
					Status = FileIndexItem.ExifStatus.Ok
				});
			await seedContext.SaveChangesAsync(cancellationToken);
		}

		await using var tenantAContext = CreateContext(new TestTenantContext
		{
			TenantId = tenantAId,
			TenantSlug = tenantASlug
		});

		var visibleItems = await tenantAContext.FileIndex
			.OrderBy(item => item.FilePath)
			.ToListAsync(cancellationToken);
		var allItemsIgnoringFilter = await tenantAContext.FileIndex
			.IgnoreQueryFilters()
			.OrderBy(item => item.FilePath)
			.ToListAsync(cancellationToken);

		Assert.HasCount(2, allItemsIgnoringFilter);
		Assert.HasCount(1, visibleItems);
		Assert.IsTrue(visibleItems.All(item => item.TenantId == tenantAId),
			"Tenant A query returned data from another tenant.");
		Assert.DoesNotContain(item => item.FilePath == "/shared/tenant-b-only.jpg", visibleItems);
	}

	private ApplicationDbContext CreateContext(ITenantContext? tenantContext = null)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite(_connection)
			.Options;
		var context = new ApplicationDbContext(options)
		{
			TenantContext = tenantContext
		};
		return context;
	}

	public void Dispose()
	{
		_connection.Dispose();
	}

	private sealed class TestTenantContext : ITenantContext
	{
		public int? TenantId { get; set; }
		public string? TenantSlug { get; set; }
	}
}



