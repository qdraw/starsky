using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.DataProtection;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.DataProtection;

[TestClass]
public class SqlXmlRepositoryTest
{
	private readonly ApplicationDbContext _dbContext;
	private readonly SqlXmlRepository _repository;

	public SqlXmlRepositoryTest()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		_repository = new SqlXmlRepository(_dbContext, serviceScope, new FakeIWebLogger());
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(SqlXmlRepositoryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public void SqlXmlRepositoryTest_GetElementNull()
	{
		var item = new DataProtectionKey { Xml = null, FriendlyName = "1" };

		_dbContext.DataProtectionKeys.RemoveRange(_dbContext
			.DataProtectionKeys);
		_dbContext.DataProtectionKeys.Add(item);
		_dbContext.SaveChanges();

		var result = _repository.GetAllElements().ToList();

		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void SqlXmlRepositoryTest_ExpectedException_NullReferenceException()
	{
		var sut = new SqlXmlRepository(null!, null!, new FakeIWebLogger());
		Assert.ThrowsException<NullReferenceException>(() => sut.GetAllElements());
		// ExpectedException NullReferenceException
	}

	[SuppressMessage("Usage", "S6602:FirstOrDefault is not Find")]
	private static MySqlException CreateMySqlException(string message)
	{
		// MySqlErrorCode errorCode, string? sqlState, string message, Exception? innerException

		var ctorLIst =
			typeof(MySqlException).GetConstructors(
				BindingFlags.Instance |
				BindingFlags.NonPublic | BindingFlags.InvokeMethod);
		// s6602
		var ctor = ctorLIst.FirstOrDefault(p =>
			p.ToString() ==
			"Void .ctor(MySqlConnector.MySqlErrorCode, System.String, System.String, System.Exception)");

		var instance =
			( MySqlException? ) ctor?.Invoke(new object[]
			{
				MySqlErrorCode.AccessDenied, "test", message, new Exception()
			});
		return instance!;
	}

	[TestMethod]
	public void GetAllElements_SqlXmlRepositoryTest_Exception()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var sut = new SqlXmlRepository(new AppDbMySqlException(options), null!,
			new FakeIWebLogger());
		Assert.ThrowsException<MySqlException>(() => sut.GetAllElements());
		// EnsureCreated is trowed as exception
	}

	[TestMethod]
	public void GetAllElements_Exception2()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var readOnlyCollection =
			new SqlXmlRepository(new GetAllElementsAppDbMySqlException2(options), null!,
					new FakeIWebLogger())
				.GetAllElements();
		Assert.AreEqual(0, readOnlyCollection.Count);
	}


	[TestMethod]
	public void SqlXmlRepositoryTest_StoreElement_HappyFlow()
	{
		_repository.StoreElement(new XElement("x1", "x1"), "hi2");

		var item = _dbContext.DataProtectionKeys.FirstOrDefault(
			p => p.FriendlyName == "hi2");

		Assert.AreEqual("hi2", item!.FriendlyName);
	}

	[TestMethod]
	public void SqlXmlRepositoryTest_StoreElement_Exception()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var logger = new FakeIWebLogger();
		var repo =
			new SqlXmlRepository(
				new StoreElementException(options), null!, logger);

		repo.StoreElement(new XElement("x3", "x3"), "hi3");

		var count = 0;
		try
		{
			count = repo.GetAllElements().Count(p => p.Name == "hi3");
		}
		catch ( DbUpdateException )
		{
			// do nothing
		}

		Assert.AreEqual(0, count);

		var error = logger.TrackedExceptions.Find(p =>
			p.Item2?.Contains("AggregateException") == true);

		Assert.IsNotNull(error);
	}

	[TestMethod]
	public void SqlXmlRepositoryTest_StoreElement_Exception_RetryLimitExceededException()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var logger = new FakeIWebLogger();
		var repo =
			new SqlXmlRepository(
				new StoreElementException2RetryLimitExceededException(options), null!, logger);

		repo.StoreElement(new XElement("x1", "x1"), "hi3");

		var error = logger.TrackedExceptions.Find(p =>
			p.Item2?.Contains("AggregateException") == true);

		var count = 0;
		try
		{
			count = repo.GetAllElements().Count(p => p.Name == "hi3");
		}
		catch ( RetryLimitExceededException )
		{
			// do nothing
		}

		Assert.AreEqual(0, count);

		Assert.IsNotNull(error);
	}

	[TestMethod]
	public void SqlXmlRepositoryTest_StoreElement_Exception_Other()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var logger = new FakeIWebLogger();
		var repo =
			new SqlXmlRepository(
				new StoreElementException2OtherException(options), null!, logger);
		Assert.ThrowsException<NullReferenceException>(() =>
			repo.StoreElement(new XElement("x1", "x1"), "hi"));
	}

	private class AppDbMySqlException : ApplicationDbContext
	{
		public AppDbMySqlException(DbContextOptions options) : base(options)
		{
		}

		public override DbSet<DataProtectionKey> DataProtectionKeys =>
			throw CreateMySqlException("0x80004005 DataProtectionKeys");

		public override DatabaseFacade Database => throw CreateMySqlException("EnsureCreated");
	}

	private class GetAllElementsAppDbMySqlException2 : ApplicationDbContext
	{
		public GetAllElementsAppDbMySqlException2(DbContextOptions options) : base(options)
		{
		}

		public override DbSet<DataProtectionKey> DataProtectionKeys =>
			throw CreateMySqlException("general");

		public override DatabaseFacade Database => throw CreateMySqlException("EnsureCreated");
	}

	private class StoreElementException : ApplicationDbContext
	{
		public StoreElementException(DbContextOptions options) : base(options)
		{
		}

		public override DbSet<DataProtectionKey> DataProtectionKeys =>
			throw new DbUpdateException("general");
	}

	private class StoreElementException2RetryLimitExceededException : ApplicationDbContext
	{
		public StoreElementException2RetryLimitExceededException(DbContextOptions options) :
			base(options)
		{
		}

		public override DbSet<DataProtectionKey> DataProtectionKeys =>
			throw new RetryLimitExceededException("general");
	}

	private class StoreElementException2OtherException : ApplicationDbContext
	{
		public StoreElementException2OtherException(DbContextOptions options) : base(options)
		{
		}

		public override DbSet<DataProtectionKey> DataProtectionKeys =>
			throw new NullReferenceException("general");
	}
}
