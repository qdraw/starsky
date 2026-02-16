using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Notifications;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.NotificationsTest;

/// <summary>
///     Error cases
/// </summary>
[TestClass]
public sealed class NotificationQueryErrorTest
{
	private static bool IsCalledDbUpdateConcurrency { get; set; }

	[TestMethod]
	public async Task AddNotification_ConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(AddNotification_ConcurrencyException))
			.Options;

		var fakeQuery = new NotificationQuery(
			new AppDbContextConcurrencyException(options) { MinCount = 1 }, new FakeIWebLogger(),
			null!);
		await fakeQuery.AddNotification("");

		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}

	[TestMethod]
	public async Task AddNotification_DoubleConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(AddNotification_DoubleConcurrencyException))
			.Options;

		var fakeQuery = new NotificationQuery(
			new AppDbContextConcurrencyException(options) { MinCount = 2 }, new FakeIWebLogger(),
			null!);
		await fakeQuery.AddNotification("");

		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}

	[TestMethod]
	public async Task AddNotification_3ConcurrencyException()
	{
		IsCalledDbUpdateConcurrency = false;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(AddNotification_3ConcurrencyException))
			.Options;

		var fakeQuery = new NotificationQuery(
			new AppDbContextConcurrencyException(options) { MinCount = 3 }, new FakeIWebLogger(),
			null!);
		await fakeQuery.AddNotification("");

		Assert.IsTrue(IsCalledDbUpdateConcurrency);
	}


	[TestMethod]
	public async Task AddNotification_ShouldHandleUniqueConstraintError()
	{
		// Arrange
		const string content = "Test notification";
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(AddNotification_ShouldHandleUniqueConstraintError))
			.Options;
		var context = new DbUpdateExceptionApplicationDbContext(options)
		{
			MinCount = 1, InnerException = new SqliteException("t", 19, 19)
		};

		// Act
		var sut = new NotificationQuery(context, new FakeIWebLogger(), null!);
		var result = await sut.AddNotification(content);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(content, result.Content);
	}

	private static MySqlException CreateMySqlException(MySqlErrorCode code,
		string message)
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
			( MySqlException? ) ctor?.Invoke(
				[code, "test", message, new Exception()]);
		return instance!;
	}

	[TestMethod]
	public async Task AddNotification_ShouldHandle_MySqlErrorCode_DuplicateKeyEntry()
	{
		// Arrange
		const string content = "Test notification";
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(AddNotification_ShouldHandleUniqueConstraintError))
			.Options;
		var context = new DbUpdateExceptionApplicationDbContext(options)
		{
			MinCount = 1,
			InnerException =
				CreateMySqlException(MySqlErrorCode.DuplicateKeyEntry,
					"Duplicate entry '1' for key 'PRIMARY'")
		};

		// Act
		var sut = new NotificationQuery(context, new FakeIWebLogger(), null!);
		var result = await sut.AddNotification(content);

		await context.DisposeAsync();

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(content, result.Content);
	}

	[TestMethod]
	public async Task AddNotification_ShouldRetry_GeneralException()
	{
		// Arrange
		const string content = "Test notification";
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(AddNotification_ShouldHandleUniqueConstraintError))
			.Options;
		var context = new DbUpdateExceptionApplicationDbContext(options)
		{
			MinCount = 1, InnerException = new AggregateException("test")
		};

		// Act
		var sut = new NotificationQuery(context, new FakeIWebLogger(), null!);
		var result = await sut.AddNotification(content);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(content, result.Content);
	}

	[TestMethod]
	public async Task AddNotification_ShouldLogError_WhenInputExceedsMaxLength()
	{
		// Arrange
		var longContent = new string('a', 5_000_001); // Create a string longer than 5,000,000
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(AddNotification_ShouldLogError_WhenInputExceedsMaxLength))
			.Options;
		var fakeLogger = new FakeIWebLogger();
		var context = new ApplicationDbContext(options);

		var sut = new NotificationQuery(context, fakeLogger, null!);

		// Act
		var result = await sut.AddNotification(longContent);

		// Assert
		Assert.Contains(log =>
				log.Item2?.Contains(NotificationQuery.ErrorMessageContentToLong) == true,
			fakeLogger.TrackedExceptions);
		Assert.AreEqual(string.Empty, result.Content);
	}

	private sealed class UpdateEntryUpdateConcurrency : IUpdateEntry
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

		public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public object GetCurrentValue(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public TProperty GetOriginalValue<TProperty>(IProperty property)
		{
			throw new NotImplementedException();
		}

		public object GetOriginalValue(IPropertyBase propertyBase)
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
		public DbContext Context { get; }

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IEntityType EntityType { get; }

		public EntityState EntityState { get; set; }

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IUpdateEntry SharedIdentityEntry { get; }
#pragma warning restore 8618
	}

	private sealed class DbUpdateExceptionApplicationDbContext(DbContextOptions options)
		: ApplicationDbContext(options)
	{
		public required int MinCount { get; set; }

		private int Count { get; set; }

		public required Exception InnerException { get; set; }

		public override Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = default)
		{
			Count++;
			if ( Count <= MinCount )
			{
				throw new DbUpdateException("t", InnerException,
					new List<IUpdateEntry>());
			}

			return Task.FromResult(Count);
		}
	}

	private sealed class AppDbContextConcurrencyException : ApplicationDbContext
	{
		public AppDbContextConcurrencyException(DbContextOptions options) : base(options)
		{
		}

		public int MinCount { get; set; }

		private int Count { get; set; }

		public override int SaveChanges()
		{
			Count++;
			if ( Count <= MinCount )
			{
				throw new DbUpdateConcurrencyException("t",
					new List<IUpdateEntry> { new UpdateEntryUpdateConcurrency() });
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
					new List<IUpdateEntry> { new UpdateEntryUpdateConcurrency() });
			}

			return Task.FromResult(Count);
		}
	}
}
