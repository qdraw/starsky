using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.DataProtection;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.DataProtection;

[TestClass]
public class SqlXmlRepositoryTest
{
	private readonly SqlXmlRepository _repository;
	private readonly ApplicationDbContext _dbContext;

	private IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(SqlXmlRepositoryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}
	
	public SqlXmlRepositoryTest()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		_repository = new SqlXmlRepository(_dbContext,serviceScope, new FakeIWebLogger());
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
	[ExpectedException(typeof(NullReferenceException))]
	public void SqlXmlRepositoryTest_ExpectedException_NullReferenceException()
	{
		new SqlXmlRepository(null!,null!, new FakeIWebLogger()).GetAllElements();
		// ExpectedException NullReferenceException
	}
	
	private static MySqlException CreateMySqlException(string message)
	{
		var info = new SerializationInfo(typeof(Exception),
			new FormatterConverter());
		info.AddValue("Number", 1);
		info.AddValue("SqlState", "SqlState");
		info.AddValue("Message", message);
		info.AddValue("InnerException", new Exception());
		info.AddValue("HelpURL", "");
		info.AddValue("StackTraceString", "");
		info.AddValue("RemoteStackTraceString", "");
		info.AddValue("RemoteStackIndex", 1);
		info.AddValue("HResult", 1);
		info.AddValue("Source", "");
		info.AddValue("WatsonBuckets",  Array.Empty<byte>() );
					
		// private MySqlException(SerializationInfo info, StreamingContext context)
		var ctor =
			typeof(MySqlException).GetConstructors(BindingFlags.Instance |
			                                       BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
		var instance =
			( MySqlException ) ctor.Invoke(new object[]
			{
				info,
				new StreamingContext(StreamingContextStates.All)
			});
		return instance;
	}
	
	private class AppDbMySqlException : ApplicationDbContext
	{
		public AppDbMySqlException(DbContextOptions options) : base(options)
		{
		}

		public override DbSet<DataProtectionKey> DataProtectionKeys => throw CreateMySqlException("0x80004005 DataProtectionKeys");

		public override DatabaseFacade Database => throw CreateMySqlException("EnsureCreated");
	}
	
	[TestMethod]
	[ExpectedException(typeof(MySqlException))]
	public void GetAllElements_SqlXmlRepositoryTest_Exception()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;

		new SqlXmlRepository(new AppDbMySqlException(options), null!, new FakeIWebLogger())
			.GetAllElements();
		// EnsureCreated is trowed as exception
	}
	
	private class GetAllElementsAppDbMySqlException2 : ApplicationDbContext
	{
		public GetAllElementsAppDbMySqlException2(DbContextOptions options) : base(options)
		{
		}
		public override DbSet<DataProtectionKey> DataProtectionKeys => throw CreateMySqlException("general");
		public override DatabaseFacade Database => throw CreateMySqlException("EnsureCreated");
	}
		
	[TestMethod]
	public void GetAllElements_Exception2()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;

		var readOnlyCollection = new SqlXmlRepository(new GetAllElementsAppDbMySqlException2(options), null!, new FakeIWebLogger())
			.GetAllElements();
		Assert.AreEqual(0,readOnlyCollection.Count);
	}
	

		
	[TestMethod]
	public void SqlXmlRepositoryTest_StoreElement()
	{
		_repository.StoreElement(new XElement("x1", "x1"), "hi");

		var item = _dbContext.DataProtectionKeys.FirstOrDefault(
			p => p.FriendlyName == "hi");
		
		Assert.AreEqual("hi", item!.FriendlyName);
	}
	
	private class StoreElementException : ApplicationDbContext
	{
		public StoreElementException(DbContextOptions options) : base(options)
		{
		}
		public override DbSet<DataProtectionKey> DataProtectionKeys => throw new DbUpdateException("general");
	}
	
	[TestMethod]
	[ExpectedException(typeof(AggregateException))]
	public void SqlXmlRepositoryTest_StoreElement_Exception()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;

		var repo =
			new SqlXmlRepository(
				new StoreElementException(options), null!, new FakeIWebLogger());
		
		repo.StoreElement(new XElement("x1", "x1"), "hi");
	}
	
}
