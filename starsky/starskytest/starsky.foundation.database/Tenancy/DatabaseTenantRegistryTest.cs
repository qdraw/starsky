using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Tenancy;

[TestClass]
public sealed class DatabaseTenantRegistryTest
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public void DatabaseNotAvailable_OnlyDefaultTenant_AndTheFailureIsLogged()
	{
		// no database context is registered: the load fails, as before the migration has run
		var services = new ServiceCollection().BuildServiceProvider();
		var logger = new FakeIWebLogger();
		var registry = new DatabaseTenantRegistry(
			services.GetRequiredService<IServiceScopeFactory>(), logger);

		Assert.IsTrue(registry.IsActive("default"));
		Assert.IsFalse(registry.IsActive("acme"));
		Assert.IsNull(registry.GetRoots("acme"));
		Assert.AreEqual("default", registry.ActiveTenantIds.Single());
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}

	/// <summary>
	///     The first caller loads (slowly), the second waits for the lock and then finds the
	///     snapshot that the first one made: the database is read once
	/// </summary>
	[TestMethod]
	public async Task ConcurrentCallers_LoadOnce()
	{
		var inner = new ServiceCollection().BuildServiceProvider();
		var scopeFactory = new BlockingScopeFactory(inner.GetRequiredService<IServiceScopeFactory>());
		var registry = new DatabaseTenantRegistry(scopeFactory, new FakeIWebLogger());

		var first = Task.Run(() => registry.IsActive("acme"), TestContext.CancellationToken);
		scopeFactory.Entered.Wait(TestContext.CancellationToken);
		var second = Task.Run(() => registry.IsActive("beta"), TestContext.CancellationToken);
		// the second passed the first check and now waits for the lock
		await Task.Delay(300, TestContext.CancellationToken);
		scopeFactory.Release.Set();

		await Task.WhenAll(first, second);

		Assert.AreEqual(1, scopeFactory.Loads);
	}

	private sealed class BlockingScopeFactory(IServiceScopeFactory inner) : IServiceScopeFactory
	{
		public readonly ManualResetEventSlim Entered = new();
		public readonly ManualResetEventSlim Release = new();
		public int Loads;

		public IServiceScope CreateScope()
		{
			Interlocked.Increment(ref Loads);
			Entered.Set();
			Release.Wait(TimeSpan.FromSeconds(10));
			return inner.CreateScope();
		}
	}
}
