using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryUpdateItemError
	{
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
			public void SetOriginalValue(IProperty property, object value)
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
		
			public void SetStoreGeneratedValue(IProperty property, object value)
			{
				throw new System.NotImplementedException();
			}
		
			public EntityEntry ToEntityEntry()
			{
				IsCalledDbUpdateConcurrency = true;
				throw new DbUpdateConcurrencyException();
				// System.NullReferenceException: Object reference not set to an instance of an object.
			}
		
			public IEntityType EntityType { get; }
			public EntityState EntityState { get; set; }
			public IUpdateEntry SharedIdentityEntry { get; }
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


			public override DbSet<FileIndexItem> FileIndex => null;

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


		[TestMethod]
		public void Query_UpdateItem_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options));
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
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options));
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
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options), null,null, null,new FakeIWebLogger());
			await fakeQuery.RemoveItemAsync(new FileIndexItem("test"));
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
				
		[TestMethod]
		public async Task Query_UpdateItemAsync_Multiple_DbUpdateConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options));
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
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options), null, null, null, new FakeIWebLogger());
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
			await context.FileIndex.AddAsync(new FileIndexItem("/test.jpg"));
			await context.SaveChangesAsync();
			var item = await context.FileIndex.FirstOrDefaultAsync(
				p => p.FilePath == "/test.jpg");

			var sqLiteFailContext = new SqliteExceptionDbContext(options);
			Assert.AreEqual(sqLiteFailContext.Count, 0);

			var fakeQuery = new Query(sqLiteFailContext, null, null, scope, new FakeIWebLogger());
			await fakeQuery.RemoveItemAsync(item);
			
			Assert.AreEqual(sqLiteFailContext.Count, 1);
		}
		
		[TestMethod]
		public async Task AddItemAsync_SQLiteException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("MovieListDatabase")
				.Options;

			var scope = CreateNewScopeSqliteException();

			var sqLiteFailContext = new SqliteExceptionDbContext(options);
			Assert.AreEqual(sqLiteFailContext.Count, 0);

			var fakeQuery = new Query(sqLiteFailContext, null, null, scope, new FakeIWebLogger());
			await fakeQuery.AddItemAsync(new FileIndexItem("/test22.jpg"));
			
			Assert.AreEqual(sqLiteFailContext.Count, 1);
		}
		
		private class FakePropertyValues : PropertyValues
		{
			public FakePropertyValues(InternalEntityEntry internalEntry) : base(internalEntry)
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

			public override object this[string propertyName]
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}

			public override object this[IProperty property]
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
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options));

			fakeQuery.SolveConcurrencyException(new FileIndexItem(),
				new FakePropertyValues(null), new FakePropertyValues(null),
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
			
			var fakeQuery = new Query(new AppDbContextConcurrencyException(options));

			fakeQuery.SolveConcurrencyException(null,
				new FakePropertyValues(null), new FakePropertyValues(null),
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
			
			var fakeQuery = new Query(appDbInvalidOperationException, null, null, scope);
			
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
			
			var fakeQuery = new Query(appDbInvalidOperationException, null, null, scope);

			testItem.Tags = "test";
			
			fakeQuery.UpdateItem(testItem);

			Assert.AreEqual("test", dbContext.FileIndex.FirstOrDefault(p => p.FilePath == "/test.jpg").Tags);
			Assert.AreEqual(2, appDbInvalidOperationException.Count);
		}
	}
}
