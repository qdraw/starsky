using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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


	private static bool IsCalledMySqlSaveDbExceptionContext { get; set; }

	[TestMethod]
	public async Task ThumbnailQuery_ConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var fakeQuery =
			new ThumbnailQuery(new AppDbContextConcurrencyException(options) { MinCount = 1 },
				null!, new FakeIWebLogger(), new FakeMemoryCache());
		await fakeQuery.RenameAsync("1", "1");

		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}

	[TestMethod]
	public async Task ThumbnailQuery_DoubleConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var fakeQuery =
			new ThumbnailQuery(new AppDbContextConcurrencyException(options) { MinCount = 2 },
				null!, new FakeIWebLogger(), new FakeMemoryCache());
		await fakeQuery.RenameAsync("1", "2");

		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}

	[TestMethod]
	public async Task ThumbnailQuery_3ConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var fakeQuery =
			new ThumbnailQuery(new AppDbContextConcurrencyException(options) { MinCount = 3 },
				null!, new FakeIWebLogger(), new FakeMemoryCache());
		await fakeQuery.RenameAsync("1", "2");

		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}

	[DataTestMethod] // [Theory]
	[DataRow(MySqlErrorCode.DuplicateKey)]
	[DataRow(MySqlErrorCode.DuplicateKeyEntry)]
	public async Task AddThumbnailRangeAsync_ShouldCatchPrimaryKeyHit(MySqlErrorCode code)
	{
		IsCalledMySqlSaveDbExceptionContext = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var fakeQuery = new ThumbnailQuery(
			new MySqlSaveDbExceptionContext(options, "Duplicate entry '1' for key 'PRIMARY'", code),
			null!, new FakeIWebLogger(), new FakeMemoryCache());

		await fakeQuery.AddThumbnailRangeAsync([new ThumbnailResultDataTransferModel("t")]);

		Assert.IsTrue(IsCalledMySqlSaveDbExceptionContext);
	}

	[TestMethod]
	public async Task AddThumbnailRangeAsync_SomethingElseShould_ExpectedException()
	{
		IsCalledMySqlSaveDbExceptionContext = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var fakeQuery = new ThumbnailQuery(
			new MySqlSaveDbExceptionContext(options, "Something else",
				MySqlErrorCode.AbortingConnection),
			null!,
			new FakeIWebLogger(), new FakeMemoryCache()
		);

		// Assert that a MySqlException is thrown when AddThumbnailRangeAsync is called
		await Assert.ThrowsExceptionAsync<MySqlException>(async () =>
			await fakeQuery.AddThumbnailRangeAsync([new ThumbnailResultDataTransferModel("t")]));
	}

	private class UpdateEntryUpdateConcurrency : IUpdateEntry
	{
		public void SetOriginalValue(IProperty property, object? value)
		{
			throw new NotImplementedException();
		}

		public void SetPropertyModified(IProperty property)
		{
			throw new NotImplementedException();
		}

		public bool IsModified(IProperty property)
		{
			throw new NotImplementedException();
		}

		public bool HasTemporaryValue(IProperty property)
		{
			throw new NotImplementedException();
		}

		public bool IsStoreGenerated(IProperty property)
		{
			throw new NotImplementedException();
		}

		public object GetCurrentValue(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public object GetOriginalValue(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public TProperty GetOriginalValue<TProperty>(IProperty property)
		{
			throw new NotImplementedException();
		}

		public void SetStoreGeneratedValue(IProperty property, object? value,
			bool setModified = true)
		{
			throw new NotImplementedException();
		}

		public EntityEntry ToEntityEntry()
		{
			IsCalledDbUpdateConcurrency = true;
			throw new DbUpdateConcurrencyException();
			// System.NullReferenceException: Object reference not set to an instance of an object.
		}

		public object GetRelationshipSnapshotValue(IPropertyBase propertyBase)
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
#pragma warning disable 8618
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
					new List<IUpdateEntry> { new UpdateEntryUpdateConcurrency() });
			}

			return Task.FromResult(Count);
		}

		public override Task AddRangeAsync(params object[] entities)
		{
			return Task.CompletedTask;
		}
	}

	private class MySqlSaveDbExceptionContext : ApplicationDbContext
	{
		private readonly string _error;
		private readonly MySqlErrorCode _key;

		public MySqlSaveDbExceptionContext(DbContextOptions options, string error,
			MySqlErrorCode key) : base(options)
		{
			_error = error;
			_key = key;
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			IsCalledMySqlSaveDbExceptionContext = true;
			throw CreateMySqlException(_error, _key);
		}

		[SuppressMessage("Usage",
			"S6602:\"Find\" method should be used instead of the \"FirstOrDefault\" extension")]
		private static MySqlException CreateMySqlException(string message, MySqlErrorCode key)
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
					key, "test", message, new Exception()
				})!;
			return instance;
		}
	}
}
