using System;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.realtime.Interface;

namespace starskytest.FakeMocks;

/// <summary>
///     Fake implementation of IServiceScopeFactory for testing
/// </summary>
internal sealed class FakeServiceScopeFactory : IServiceScopeFactory
{
	private readonly Func<IServiceScope>? _create;
	private readonly ServiceCollection _services = [];

	public FakeServiceScopeFactory(Func<IServiceScope>? create)
	{
		_create = create;
	}

	public FakeServiceScopeFactory(ServiceCollection services)
	{
		_services = services;
	}

	public int CreateScopeCount { get; private set; }


	public IServiceScope CreateScope()
	{
		CreateScopeCount++;
		if ( _create != null )
		{
			return _create();
		}

		var services = _services.AddSingleton<IRealtimeConnectionsService>(
			new FakeIRealtimeConnectionsService());
		return services.BuildServiceProvider().CreateScope();
	}
}
