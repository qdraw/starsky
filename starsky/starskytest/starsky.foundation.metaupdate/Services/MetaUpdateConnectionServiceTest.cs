using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.realtime.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.metaupdate.Services;

[TestClass]
public sealed class MetaUpdateConnectionServiceTest
{
	[TestMethod]
	public async Task Constructor_ResolvesServices_FromScopeFactory_And_UpdateCallsSendAndAdd()
	{
		var fakeWs = new FakeIWebSocketConnectionsService();
		var fakeNq = new FakeINotificationQuery();

		// Create a provider and scope; factory will return scopes via the lambda
		var provider1 = new SimpleProvider(fakeWs, fakeNq);
		var scope1 = new FakeServiceScope(provider1);
		var factory = new FakeServiceScopeFactory(() => scope1);

		// First, test that when factory returns the same scope, CreateScope is invoked by constructor twice
		var service = new MetaUpdateConnectionService(factory);

		// The service was constructed; now call UpdateWebSocketTaskRun
		var items = new List<FileIndexItem> { new("/x.jpg") };
		var result = await service.UpdateWebSocketTaskRun(items);

		Assert.IsNotNull(result);
		Assert.HasCount(1, fakeWs.FakeSendToAllAsync,
			"Expected SendToAllAsync to be called on IWebSocketConnectionsService");
		Assert.IsTrue(fakeNq.Added, "Expected AddNotification to be called on INotificationQuery");
	}

	[TestMethod]
	public void Constructor_CreatesTwoScopes_OnConstruction()
	{
		var fakeWs = new FakeIWebSocketConnectionsService();
		var fakeNq = new FakeINotificationQuery();

		var provider = new SimpleProvider(fakeWs, fakeNq);

		var scope = new FakeServiceScope(provider);
		var factory = new FakeServiceScopeFactory(() => scope);

		// construct
		_ = new MetaUpdateConnectionService(factory);

		Assert.AreEqual(2, factory.CreateScopeCount,
			"Expected CreateScope to be called twice in constructor");
	}

	private class FakeServiceScope : IServiceScope
	{
		public FakeServiceScope(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public IServiceProvider ServiceProvider { get; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool _)
		{
			// nothing to dispose in this fake
		}
	}

	private class FakeServiceScopeFactory : IServiceScopeFactory
	{
		private readonly Func<IServiceScope> _create;

		public FakeServiceScopeFactory(Func<IServiceScope> create)
		{
			_create = create;
		}

		public int CreateScopeCount { get; private set; }

		public IServiceScope CreateScope()
		{
			CreateScopeCount++;
			return _create();
		}
	}

	private class SimpleProvider : IServiceProvider
	{
		private readonly object _nq;
		private readonly object _ws;

		public SimpleProvider(object ws, object nq)
		{
			_ws = ws;
			_nq = nq;
		}

		public object GetService(Type serviceType)
		{
			if ( serviceType == typeof(IWebSocketConnectionsService) )
			{
				return _ws;
			}

			if ( serviceType == typeof(INotificationQuery) )
			{
				return _nq;
			}

			return null!;
		}
	}
}
