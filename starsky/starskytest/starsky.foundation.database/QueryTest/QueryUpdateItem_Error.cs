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
	public sealed class QueryUpdateItemError
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
		
		private class AppDbContextDoubleConcurrencyException : ApplicationDbContext
		{
			public AppDbContextDoubleConcurrencyException(DbContextOptions options) : base(options)
			{
			}

			public int Count { get; set; }

			public override int SaveChanges()
			{
				Count++;
				if ( Count <= 2 )
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
				if ( Count <= 2 )
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
		public async Task Query_UpdateItem_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null!,null!,null!);
			await fakeQuery.UpdateItemAsync(new FileIndexItem());
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public async Task Query_UpdateItemAsync_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null!,null!,null!);
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
				null!, null!,new FakeIWebLogger());
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
				null!, _serviceScopeFactory! ,new FakeIWebLogger());
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
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null!,null!,null!);
			await fakeQuery.UpdateItemAsync(new List<FileIndexItem>{new FileIndexItem("test")});
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}

		[TestMethod]
		public async Task RemoveItemAsync_SingleItem_SQLiteException()
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

			var fakeQuery = new Query(sqLiteFailContext, new AppSettings(), scope, new FakeIWebLogger());
			await fakeQuery.RemoveItemAsync(item!);
			
			Assert.AreEqual(1, sqLiteFailContext.Count);
		}
		
		[TestMethod]
		public async Task RemoveItemAsync_List_SQLiteException()
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

			var fakeQuery = new Query(sqLiteFailContext, new AppSettings(), scope, new FakeIWebLogger());
			await fakeQuery.RemoveItemAsync(new List<FileIndexItem>{item!});
			
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

			var fakeQuery = new Query(sqLiteFailContext, new AppSettings(), scope, new FakeIWebLogger());
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
			
			SolveConcurrency.SolveConcurrencyException(null!,
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

			public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
			{
				if ( Count == 1 )
				{
					Count++;
					throw new InvalidOperationException("test");
				}
				return Task.FromResult(0);
			}
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
		public async Task Query_UpdateItem_List_InvalidOperationException()
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
			
			var fakeQuery = new Query(appDbInvalidOperationException,  new AppSettings(), scope,new FakeIWebLogger());
			
			await fakeQuery.UpdateItemAsync(new List<FileIndexItem>());

			Assert.AreEqual(1, appDbInvalidOperationException.Count);
		}
		
		[TestMethod]
		public async Task Query_UpdateItem_1_InvalidOperationException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase2")
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
			await dbContext.SaveChangesAsync();
			
			var fakeQuery = new Query(appDbInvalidOperationException,  new AppSettings(), scope,new FakeIWebLogger());

			testItem.Tags = "test";
			
			await fakeQuery.UpdateItemAsync(testItem);
			
			Assert.AreEqual(2, appDbInvalidOperationException.Count);
		}
		
		[TestMethod]
		public async Task QueryRemoveItemAsyncTest_SingleItem_InvalidOperationException_SingleItem_AddOneItem()
		{
			const string path = "/QueryRemoveItemAsyncTest_InvalidOperationException_SingleItem_AddOneItem";

			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase2")
				.Options;

			var appDbInvalidOperationException =
				new AppDbInvalidOperationException(options);
			var services = new ServiceCollection();
			services.AddSingleton(new ApplicationDbContext(options));
			var serviceProvider = services.BuildServiceProvider();
			var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var dbContext = scope.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var testItem = new FileIndexItem(path);
			dbContext.FileIndex.Add(testItem);
			await dbContext.SaveChangesAsync();
			
			var query = new Query(appDbInvalidOperationException,  new AppSettings(), scope,new FakeIWebLogger());

			await query.RemoveItemAsync(testItem);
			
			var afterResult = await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == path);
			Assert.AreEqual(null, afterResult);
		}
		
				
		[TestMethod]
		public async Task QueryRemoveItemAsyncTest_List_InvalidOperationException()
		{
			const string path1 = "/QueryRemoveItemAsyncTest_List_InvalidOperationException__1";
			const string path2 = "/QueryRemoveItemAsyncTest_List_InvalidOperationException__2";

			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase2")
				.Options;

			var appDbInvalidOperationException =
				new AppDbInvalidOperationException(options);
			var services = new ServiceCollection();
			services.AddSingleton(new ApplicationDbContext(options));
			var serviceProvider = services.BuildServiceProvider();
			var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var dbContext = scope.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var testItem1 = new FileIndexItem(path1);
			dbContext.FileIndex.Add(testItem1);
			var testItem2 = new FileIndexItem(path2);
			dbContext.FileIndex.Add(testItem2);
			await dbContext.SaveChangesAsync();
			
			var query = new Query(appDbInvalidOperationException,  new AppSettings(), scope,new FakeIWebLogger());

			await query.RemoveItemAsync(new List<FileIndexItem>{testItem1,testItem2});
			
			var afterResult1 = await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == path1);
			Assert.AreEqual(null, afterResult1);
			
			var afterResult2 = await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == path2);
			Assert.AreEqual(null, afterResult2);
		}
		
		[TestMethod]
		public async Task Query_RemoveItemAsync_List_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null!,null!,new FakeIWebLogger());
			await fakeQuery.RemoveItemAsync(new List<FileIndexItem>{new FileIndexItem("test")});
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
				
		[TestMethod]
		public async Task Query_AddRangeAsync_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null!,null!,new FakeIWebLogger());
			
			var fileIndexItemList = new List<FileIndexItem>
			{
				new FileIndexItem { FilePath = "test1.jpg" },
				new FileIndexItem { FilePath = "test2.jpg" }
			};
			
			await fakeQuery.AddRangeAsync(fileIndexItemList);
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public async Task Query_AddRangeAsync_DoubleConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options),null!,null!,new FakeIWebLogger());
			
			var fileIndexItemList = new List<FileIndexItem>
			{
				new FileIndexItem { FilePath = "test1.jpg" },
				new FileIndexItem { FilePath = "test2.jpg" }
			};
			
			await fakeQuery.AddRangeAsync(fileIndexItemList);
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public async Task Query_AddRangeAsync_DoubleConcurrencyException_Verbose()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextDoubleConcurrencyException(options),new AppSettings
			{
				Verbose = true
			},null!,new FakeIWebLogger());
			
			var fileIndexItemList = new List<FileIndexItem>
			{
				new FileIndexItem { FilePath = "test1.jpg" },
				new FileIndexItem { FilePath = "test2.jpg" }
			};
			
			// verbose
			await fakeQuery.AddRangeAsync(fileIndexItemList);
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
				
		[TestMethod]
		public async Task Query_AddRangeAsync_DbUpdateConcurrencyException_Verbose_NoLogger()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextDoubleConcurrencyException(options),new AppSettings
			{
				Verbose = true
			},null!,null!);
			
			var fileIndexItemList = new List<FileIndexItem>
			{
				new FileIndexItem { FilePath = "test1.jpg" },
				new FileIndexItem { FilePath = "test2.jpg" }
			};
			
			await fakeQuery.AddRangeAsync(fileIndexItemList);
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
	}
}
