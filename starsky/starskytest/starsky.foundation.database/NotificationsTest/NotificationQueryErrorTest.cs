#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Notifications;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.NotificationsTest
{
	
	[TestClass]
	public sealed class NotificationQueryErrorTest
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
			public DbContext Context { get; }

			// ReSharper disable once UnassignedGetOnlyAutoProperty
			public IEntityType EntityType { get; }

			public EntityState EntityState { get; set; }

			// ReSharper disable once UnassignedGetOnlyAutoProperty
			public IUpdateEntry SharedIdentityEntry { get; }
#pragma warning restore 8618
		}
	
		private class AppDbContextConcurrencyException : ApplicationDbContext
		{
			public AppDbContextConcurrencyException(DbContextOptions options) : base(options)
			{
			}

			public int MinCount { get; set; }
			public int Count { get; set; }

			public override int SaveChanges()
			{
				Count++;
				if ( Count <= MinCount )
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
				if ( Count <= MinCount )
				{
					throw new DbUpdateConcurrencyException("t",
						new List<IUpdateEntry>{new UpdateEntryUpdateConcurrency()});
				}
				return Task.FromResult(Count);
			}
		}
		
		[TestMethod]
		public async Task AddNotification_ConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new NotificationQuery(new AppDbContextConcurrencyException(options)
			{
				MinCount = 1
			},new FakeIWebLogger(),null!);
			await fakeQuery.AddNotification("");
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public async Task AddNotification_DoubleConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new NotificationQuery(new AppDbContextConcurrencyException(options)
			{
				MinCount = 2
			},new FakeIWebLogger(),null!);
			await fakeQuery.AddNotification("");
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
		
		[TestMethod]
		public async Task AddNotification_3ConcurrencyException()
		{
			IsCalledDbUpdateConcurrency = false;
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var fakeQuery = new NotificationQuery(new AppDbContextConcurrencyException(options)
			{
				MinCount = 3
			},new FakeIWebLogger(),null!);
			await fakeQuery.AddNotification("");
			
			Assert.IsTrue(IsCalledDbUpdateConcurrency);
		}
	}

}
