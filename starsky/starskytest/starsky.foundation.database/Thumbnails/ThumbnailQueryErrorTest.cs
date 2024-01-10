#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Thumbnails;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Thumbnails;

[TestClass]
public class ThumbnailQueryErrorTest
{
	private static bool IsCalledDbUpdateConcurrency { get; set; }

	private class UpdateEntryUpdateConcurrency : IUpdateEntry
	{
		public void SetOriginalValue(IProperty property, object? value)
		{
			throw new System.NotImplementedException();
		}
		
		public void SetPropertyModified(IProperty property)
		{
			throw new System.NotImplementedException();
		}
		
		public bool IsModified(IProperty property)
		{
			throw new System.NotImplementedException();
		}
		
		public bool HasTemporaryValue(IProperty property)
		{
			throw new System.NotImplementedException();
		}
		
		public bool IsStoreGenerated(IProperty property)
		{
			throw new System.NotImplementedException();
		}
		
		public object GetCurrentValue(IPropertyBase propertyBase)
		{
			throw new System.NotImplementedException();
		}
		
		public object GetOriginalValue(IPropertyBase propertyBase)
		{
			throw new System.NotImplementedException();
		}
		
		public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
		{
			throw new System.NotImplementedException();
		}
		
		public TProperty GetOriginalValue<TProperty>(IProperty property)
		{
			throw new System.NotImplementedException();
		}

		public void SetStoreGeneratedValue(IProperty property, object? value,
			bool setModified = true)
		{
			throw new NotImplementedException();
		}

		public void SetStoreGeneratedValue(IProperty property, object? value)
		{
			throw new NotImplementedException();
		}
		
		public EntityEntry ToEntityEntry()
		{
			IsCalledDbUpdateConcurrency = true;
			throw new DbUpdateConcurrencyException();
			// System.NullReferenceException: Object reference not set to an instance of an object.
		}

		public object? GetRelationshipSnapshotValue(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public object GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public bool IsConceptualNull(IProperty property)
		{
			throw new NotImplementedException();
		}

		public DbContext Context { get; }

#pragma warning disable 8618
		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IEntityType EntityType { get; }
#pragma warning restore 8618
		public EntityState EntityState { get; set; }
#pragma warning disable 8618
		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IUpdateEntry SharedIdentityEntry { get; }
#pragma warning restore 8618
	}
	
	private class AppDbContextConcurrencyException : ApplicationDbContext
	{
		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public AppDbContextConcurrencyException(DbContextOptions options) : base(options)
		{
			Thumbnails.Add(new ThumbnailItem("1", true, true, true, true));
			try
			{
				SaveChanges();
			}
			catch ( ArgumentException )
			{
				// An item with the same key has already been added. Key: 1
			}
		}

		public int MinCount { get; set; }
		public int Count { get; set; }

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			Count++;
			if ( Count <= MinCount )
			{
				throw new DbUpdateConcurrencyException("t",
					new List<IUpdateEntry>{new UpdateEntryUpdateConcurrency()});
			}
			return Task.FromResult(Count);
		}	
			
		public override Task AddRangeAsync(params object[] entities)
		{
			return Task.CompletedTask;
		}	
	}
			
	[TestMethod]
	public async Task ThumbnailQuery_ConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;
			
		var fakeQuery = new ThumbnailQuery(new AppDbContextConcurrencyException(options)
		{
			MinCount = 1
		},null!,new FakeIWebLogger());
		await fakeQuery.RenameAsync("1","1");
			
		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}
		
	[TestMethod]
	public async Task ThumbnailQuery_DoubleConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;
			
		var fakeQuery = new ThumbnailQuery(new AppDbContextConcurrencyException(options)
		{
			MinCount = 2
		},null!,new FakeIWebLogger());
		await fakeQuery.RenameAsync("1","2");
			
		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}
		
	[TestMethod]
	public async Task ThumbnailQuery_3ConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;
			
		var fakeQuery = new ThumbnailQuery(new AppDbContextConcurrencyException(options)
		{
			MinCount = 3
		},null!,new FakeIWebLogger());
		await fakeQuery.RenameAsync("1","2");
			
		Assert.IsTrue(IsCalledDbUpdateConcurrency);
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
			( MySqlException? ) ctor?.Invoke(new object[]
			{
				info,
				new StreamingContext(StreamingContextStates.All)
			});
		return instance!;
	}
	private static bool IsCalledMySqlSaveDbExceptionContext { get; set; }

	private class MySqlSaveDbExceptionContext : ApplicationDbContext
	{
		private readonly string _error;

		public MySqlSaveDbExceptionContext(DbContextOptions options, string error) : base(options)
		{
			_error = error;
		}
		
		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			IsCalledMySqlSaveDbExceptionContext = true;
			throw CreateMySqlException(_error);
		}
	}
	
	[TestMethod]
	public async Task AddThumbnailRangeAsync_ShouldCatchPrimaryKeyHit()
	{
		IsCalledMySqlSaveDbExceptionContext = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;
			
		var fakeQuery = new ThumbnailQuery(new MySqlSaveDbExceptionContext(options,"Duplicate entry '1' for key 'PRIMARY'"),
			null!,new FakeIWebLogger());
		
		await fakeQuery.AddThumbnailRangeAsync(new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("t")
		});
			
		Assert.IsTrue(IsCalledMySqlSaveDbExceptionContext);
	}
	
	[TestMethod]
	[ExpectedException(typeof(MySqlException))]
	public async Task AddThumbnailRangeAsync_SomethingElseShould_ExpectedException()
	{
		IsCalledMySqlSaveDbExceptionContext = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;
			
		var fakeQuery = new ThumbnailQuery(new MySqlSaveDbExceptionContext(options,"Something else"),
			null!,new FakeIWebLogger());
		
		await fakeQuery.AddThumbnailRangeAsync(new List<ThumbnailResultDataTransferModel>
		{
			new ThumbnailResultDataTransferModel("t")
		});
	}
}
