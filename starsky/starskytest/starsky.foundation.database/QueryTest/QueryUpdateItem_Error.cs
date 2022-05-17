#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	/// <summary>
	/// QueryUpdateItem_Error
	/// </summary>
	[TestClass]
	public class QueryUpdateItemError
	{
		private IServiceScopeFactory? _serviceScopeFactory;

		private IServiceScopeFactory CreateNewScopeSqliteException()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
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
		
			public void SetStoreGeneratedValue(IProperty property, object? value)
			{
				throw new System.NotImplementedException();
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

			public object? GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
			{
				throw new NotImplementedException();
			}

			public bool IsConceptualNull(IProperty property)
			{
				throw new NotImplementedException();
			}

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
			public AppDbContextConcurrencyException(DbContextOptions options) : base(options)
			{
			}

			public int Count { get; set; }

			public override int SaveChanges()
			{
				Count++;
				if ( Count == 1 )
				{
					throw new DbUpdateConcurrencyException("t",
						new List<IUpdateEntry>{new UpdateEntryUpdateConcurrency()});
				}
				return Count;
			}	
			
			public override Task<int> SaveChangesAsync(
				CancellationToken cancellationToken = default)
			{
				Count++;
				if ( Count == 1 )
				{
					throw new DbUpdateConcurrencyException("t",
						new List<IUpdateEntry>{new UpdateEntryUpdateConcurrency()});
				}
				return Task.FromResult(Count);
			}
		}
		
		private class SqliteExceptionDbContext : ApplicationDbContext
		{
			public SqliteExceptionDbContext(DbContextOptions options) : base(options)
			{
			}

			public int Count { get; set; }


#pragma warning disable 8603
			public override DbSet<FileIndexItem> FileIndex => null;
#pragma warning restore 8603

			public override int SaveChanges()
			{
				Count++;
				if ( Count == 1 )
				{
					throw new Microsoft.Data.Sqlite.SqliteException("t",1,2);
				}
				return Count;
			}	
			
			public override Task<int> SaveChangesAsync(
				CancellationToken cancellationToken = default)
			{
				Count++;
				if ( Count == 1 )
				{
					throw new Microsoft.Data.Sqlite.SqliteException("t",1,2);
				}
				return Task.FromResult(Count);
			}
		}

		public static int InvalidOperationExceptionDbContextCount { get; set; }
		
		private class InvalidOperationExceptionDbContext : ApplicationDbContext
		{
			public InvalidOperationExceptionDbContext(DbContextOptions options) : base(options)
			{
			}
			
			public override Task<int> SaveChangesAsync(
				CancellationToken cancellationToken = default)
			{
				InvalidOperationExceptionDbContextCount++;
				throw new InvalidOperationException("from test");
			}
		}

		[TestMethod]
		public void Query_UpdateItem_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null,null,null,null);
			fakeQuery.UpdateItem(new FileIndexItem());
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public async Task Query_UpdateItemAsync_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null,null,null,null);
			await fakeQuery.UpdateItemAsync(new FileIndexItem("test"));
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public async Task Query_RemoveItemAsync_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options), 
				null, null,new FakeIWebLogger(),null);
			await fakeQuery.RemoveItemAsync(new FileIndexItem("test"));
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}

		private void CreateScope()
		{
			var services = new ServiceCollection();
			services.AddSingleton<AppSettings>();

			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseInMemoryDatabase(nameof(CreateScope)));
			
			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task UpdateItemAsync_InvalidOperationExceptionDbContext()
		{
			InvalidOperationExceptionDbContextCount = 0;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			CreateScope();
			var fakeQuery = new Query(new InvalidOperationExceptionDbContext(options), 
				null, _serviceScopeFactory ,new FakeIWebLogger(),null);
			await fakeQuery.UpdateItemAsync(new FileIndexItem("test"));
			
			Assert.IsTrue(InvalidOperationExceptionDbContextCount == 1);
		}

				
		[TestMethod]
		public async Task Query_UpdateItemAsync_Multiple_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null,null,null,null);
			await fakeQuery.UpdateItemAsync(new List<FileIndexItem>{new FileIndexItem("test")});
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public void Query_RemoveItem_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options), null,  null, new FakeIWebLogger(), null);
			fakeQuery.RemoveItem(new FileIndexItem());
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}

		[TestMethod]
		public async Task RemoveItemAsync_SQLiteException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("MovieListDatabase")
				.Options;

			var scope = CreateNewScopeSqliteException();
			var context = scope.CreateScope().ServiceProvider
				.GetService<ApplicationDbContext>();
			if ( context == null )
			{
				throw new NullReferenceException(
					"test context should not be null");
			}
			await context.FileIndex.AddAsync(new FileIndexItem("/test.jpg"));
			await context.SaveChangesAsync();
			var item = await context.FileIndex.FirstOrDefaultAsync(
				p => p.FilePath == "/test.jpg");

			var sqLiteFailContext = new SqliteExceptionDbContext(options);
			Assert.AreEqual(0,sqLiteFailContext.Count);

			var fakeQuery = new Query(sqLiteFailContext, null, scope, new FakeIWebLogger());
			await fakeQuery.RemoveItemAsync(item);
			
			Assert.AreEqual(1, sqLiteFailContext.Count);
		}
		
		[TestMethod]
		public async Task AddItemAsync_SQLiteException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("MovieListDatabase")
				.Options;

			var scope = CreateNewScopeSqliteException();

			var sqLiteFailContext = new SqliteExceptionDbContext(options);
			Assert.AreEqual(0,sqLiteFailContext.Count);

			var fakeQuery = new Query(sqLiteFailContext, null, scope, new FakeIWebLogger());
			await fakeQuery.AddItemAsync(new FileIndexItem("/test22.jpg"));
			
			Assert.AreEqual(1, sqLiteFailContext.Count);
		}
		
		private class FakePropertyValues : PropertyValues
		{
#pragma warning disable EF1001
			public FakePropertyValues(InternalEntityEntry internalEntry) : base(internalEntry)
#pragma warning restore EF1001
			{
			}

			public override object ToObject()
			{
				throw new NotImplementedException();
			}

			public override void SetValues(object obj)
			{
				throw new NotImplementedException();
			}

			public override PropertyValues Clone()
			{
				throw new NotImplementedException();
			}

			public override void SetValues(PropertyValues propertyValues)
			{
				throw new NotImplementedException();
			}

			public override TValue GetValue<TValue>(string propertyName)
			{
				throw new NotImplementedException();
			}

			public override TValue GetValue<TValue>(IProperty property)
			{
				throw new NotImplementedException();
			}

			public override IReadOnlyList<IProperty> Properties { get; } =
				new List<IProperty>();

			public override object? this[string propertyName]
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}

			public override object? this[IProperty property]
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}
		}

		public bool IsWrittenConcurrencyException { get; set; }

		[TestMethod]
		public void SolveConcurrencyException_should_callDelegate()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			SolveConcurrency.SolveConcurrencyException(new FileIndexItem(),
#pragma warning disable 8625
				new FakePropertyValues(null), new FakePropertyValues(null),
#pragma warning restore 8625
				"", values => IsWrittenConcurrencyException = true);
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void Query_UpdateItem_NotSupportedException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			SolveConcurrency.SolveConcurrencyException(null,
#pragma warning disable 8625
				new FakePropertyValues(null), new FakePropertyValues(null),
#pragma warning restore 8625
				"", values => IsWrittenConcurrencyException = true);
			// expect error
		}
		
		
		private class AppDbInvalidOperationException : ApplicationDbContext
		{
			public AppDbInvalidOperationException(DbContextOptions options) : base(options)
			{
			}

			internal int Count { get; set; } = 1;

			public override int SaveChanges()
			{
				if ( Count == 1 )
				{
					Count++;
					throw new InvalidOperationException("test");
				}
				return 0;
			}	
		}
        
		[TestMethod]
		public void Query_UpdateItem_List_InvalidOperationException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;

			var appDbInvalidOperationException =
				new AppDbInvalidOperationException(options);
			var services = new ServiceCollection();
			services.AddSingleton(new ApplicationDbContext(options));
			var serviceProvider = services.BuildServiceProvider();
			var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var fakeQuery = new Query(appDbInvalidOperationException,  null, scope,null);
			
			fakeQuery.UpdateItem(new List<FileIndexItem>());

			Assert.AreEqual(2, appDbInvalidOperationException.Count);
		}
		
		[TestMethod]
		public void Query_UpdateItem_1_InvalidOperationException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;

			var appDbInvalidOperationException =
				new AppDbInvalidOperationException(options);
			var services = new ServiceCollection();
			services.AddSingleton(new ApplicationDbContext(options));
			var serviceProvider = services.BuildServiceProvider();
			var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var dbContext = scope.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var testItem = new FileIndexItem("/test.jpg");
			dbContext.FileIndex.Add(testItem);
			dbContext.SaveChanges();
			
			var fakeQuery = new Query(appDbInvalidOperationException,  null, scope,null);

			testItem.Tags = "test";
			
			fakeQuery.UpdateItem(testItem);

			Assert.AreEqual("test", dbContext.FileIndex.FirstOrDefault(p => p.FilePath == "/test.jpg")?.Tags);
			Assert.AreEqual(2, appDbInvalidOperationException.Count);
		}
	}
}
