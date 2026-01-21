using Microsoft.Extensions.DependencyInjection;
using starsky.feature.realtime.Interface;

namespace starskytest.FakeMocks;

/// <summary>
///     Fake implementation of IServiceScopeFactory for testing
/// </summary>
internal sealed class FakeServiceScopeFactory : IServiceScopeFactory
{
	private readonly ServiceCollection _services = [];

	public IServiceScope CreateScope()
	{
		var services = _services.AddSingleton<IRealtimeConnectionsService>(
			new FakeIRealtimeConnectionsService());
		return services.BuildServiceProvider().CreateScope();
	}
}
