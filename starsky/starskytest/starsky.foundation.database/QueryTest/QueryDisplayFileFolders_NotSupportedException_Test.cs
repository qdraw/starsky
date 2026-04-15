using System;
using System.Diagnostics.CodeAnalysis;
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
public class QueryDisplayFileFolders_NotSupportedException_Test
{
	[TestMethod]
	public void
		QueryDisplayFileFolders_uses_scope_when_primary_context_throws_NotSupportedException()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;

		// Primary context that throws when FileIndex is accessed
		var primary = new ThrowingFileIndexDbContext(options);

		// Create an in-memory context to be returned by the scope
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
		var scopeContext = new ApplicationDbContext(builder.Options);
		var expectedItem =
			new FileIndexItem("/file.jpg") { FileName = "file.jpg", ParentDirectory = "/" };
		scopeContext.FileIndex.Add(expectedItem);
		scopeContext.SaveChanges();

		// Fake IServiceScopeFactory -> IServiceScope -> IServiceProvider
		var fakeScopeFactory = new FakeServiceScopeFactory(scopeContext);

		var appSettings = new AppSettings();
		var fakeLogger = new FakeIWebLogger();

		var query = new Query(primary, appSettings, fakeScopeFactory, fakeLogger);

		// Act
		var result = query.QueryDisplayFileFolders();

		// Assert - result should come from the scopeContext (the in-memory DB)
		Assert.IsNotNull(result);
		Assert.Contains(r => r.FilePath == expectedItem.FilePath, result);
		Assert.AreEqual(1, fakeScopeFactory.CreateCount);
	}

	// Primary context that throws when FileIndex is accessed
	internal class ThrowingFileIndexDbContext : ApplicationDbContext
	{
		public ThrowingFileIndexDbContext(DbContextOptions options) : base(options)
		{
		}

		[SuppressMessage("Usage", "S3237:S3237",
			Justification = "Is checked")]
		public override DbSet<FileIndexItem> FileIndex
		{
			get => throw new NotSupportedException("Simulated EF Core read conflict");
			set
			{
				// do nothing
				/* no-op */
			}
		}
	}

	// // Minimal fake service provider/scope factory to return our in-memory context
	internal class FakeServiceProvider : IServiceProvider
	{
		private readonly ApplicationDbContext _ctx;

		public FakeServiceProvider(ApplicationDbContext ctx)
		{
			_ctx = ctx;
		}

		public object? GetService(Type serviceType)
		{
			return serviceType == typeof(ApplicationDbContext) ? _ctx : null;
		}
	}

	internal class FakeServiceScopeFactory(ApplicationDbContext ctx) : IServiceScopeFactory
	{
		public int CreateCount { get; private set; }

		public int CreateCountPublic => CreateCount;

		public IServiceScope CreateScope()
		{
			CreateCount++;
			return new FakeServiceScope(new FakeServiceProvider(ctx));
		}
	}
}
