using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryAddParentItemsFallbackTest
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public async Task AddParentItemsAsync_PrimaryContextFails_UsesScopedFallback()
	{
		var primary = new ThrowingDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
			.Options);
		var scopeFactory = new FakeIServiceScopeFactory(nameof(QueryAddParentItemsFallbackTest));
		var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		dbContext.FileIndex.Add(new FileIndexItem("/")
		{
			IsDirectory = true, ParentDirectory = "/", FileName = "/"
		});
		await dbContext.SaveChangesAsync(TestContext.CancellationToken);

		var query = new Query(primary, new AppSettings { AddMemoryCache = false }, scopeFactory,
			new FakeIWebLogger());

		// the root exists in the fallback database: nothing is left to add
		var added = await query.AddParentItemsAsync("/");

		Assert.IsEmpty(added);
	}

	[TestMethod]
	public void QueryFactory_TypeWithoutMatchingConstructor_Throws()
	{
		var factory = new QueryFactory(null, new FakeQueryWithoutFactoryConstructor(), null, null,
			null, null);

		Assert.ThrowsExactly<MissingMethodException>(() => factory.Query());
	}

	private sealed class FakeQueryWithoutFactoryConstructor : FakeIQuery
	{
		public FakeQueryWithoutFactoryConstructor() : base([])
		{
		}
	}

	private sealed class ThrowingDbContext(DbContextOptions options)
		: ApplicationDbContext(options)
	{
		public override DbSet<FileIndexItem> FileIndex
		{
			get => throw new InvalidOperationException("Simulated ExecuteReader error");
			set
			{
				// do nothing here
			}
		}
	}
}
