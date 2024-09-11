using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryGetAllRecursiveTest
{
	private readonly Query _query;

	public QueryGetAllRecursiveTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		_query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, new FakeIWebLogger(), memoryCache);
	}

	private static bool IsCalledMySqlSaveDbExceptionContext { get; set; }

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryGetAllFilesTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task ShouldGiveMultipleResultsBack()
	{
		await _query.AddItemAsync(
			new FileIndexItem("/recursive_test/image0.jpg"));
		await _query.AddItemAsync(
			new FileIndexItem("/recursive_test/sub/image1.jpg"));
		await _query.AddItemAsync(
			new FileIndexItem("/recursive_test/image2.jpg"));
		await _query.AddItemAsync(
			new FileIndexItem("/recursive_test/image3.jpg"));


		var result = await _query.GetAllRecursiveAsync(new List<string> { "/recursive_test/" });

		Assert.AreEqual("/recursive_test/image0.jpg", result[0].FilePath);
		Assert.AreEqual("/recursive_test/image2.jpg", result[1].FilePath);
		Assert.AreEqual("/recursive_test/image3.jpg", result[2].FilePath);
		Assert.AreEqual("/recursive_test/sub/image1.jpg", result[3].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
		await _query.RemoveItemAsync(result[2]);
		await _query.RemoveItemAsync(result[3]);
	}

	[SuppressMessage("Usage",
		"S6602:\"Find\" method should be used instead of the \"FirstOrDefault\" extension")]
	[SuppressMessage("Usage", "S3398:move class")]
	private static MySqlException CreateMySqlException(MySqlErrorCode code, string message)
	{
		// MySqlErrorCode errorCode, string? sqlState, string message, Exception? innerException

		var ctorLIst =
			typeof(MySqlException).GetConstructors(
				BindingFlags.Instance |
				BindingFlags.NonPublic | BindingFlags.InvokeMethod);
		var ctor = ctorLIst.FirstOrDefault(p =>
			p.ToString() ==
			"Void .ctor(MySqlConnector.MySqlErrorCode, System.String, System.String, System.Exception)");

		var instance =
			( MySqlException ) ctor?.Invoke(new object[]
			{
				code, "test", message, new Exception()
			})!;
		return instance;
	}

	[DataTestMethod] // [Theory]
	[DataRow(MySqlErrorCode.QueryTimeout)]
	[DataRow(MySqlErrorCode.QueryInterrupted)]
	public async Task Retry_When_HitTimeout(MySqlErrorCode code)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;
		var fakeQuery = new Query(new MySqlSaveDbExceptionContext(options, code),
			null!, CreateNewScope(), new FakeIWebLogger());

		await fakeQuery.GetAllRecursiveAsync("test");

		Assert.IsTrue(IsCalledMySqlSaveDbExceptionContext);
	}

	[TestMethod]
	[ExpectedException(typeof(MySqlException))]
	public async Task QueryGetAllRecursive_GeneralException()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;
		var fakeQuery = new Query(new MySqlSaveDbExceptionContext(options, MySqlErrorCode.None),
			null!, CreateNewScope(), new FakeIWebLogger());

		await fakeQuery.GetAllRecursiveAsync("test");
	}

	private class MySqlSaveDbExceptionContext : ApplicationDbContext
	{
		private readonly MySqlErrorCode _error;

		public MySqlSaveDbExceptionContext(DbContextOptions options,
			MySqlErrorCode error) : base(options)
		{
			_error = error;
		}

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		public DbSet<FileIndexItem> IndexItems { get; set; } = null!;

		public override DbSet<FileIndexItem> FileIndex
		{
			get
			{
				IsCalledMySqlSaveDbExceptionContext = true;
				throw CreateMySqlException(_error, "test error");
			}
			set => IndexItems = value;
		}
	}
}
