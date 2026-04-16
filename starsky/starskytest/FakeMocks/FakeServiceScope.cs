using System;
using Microsoft.Extensions.DependencyInjection;

namespace starskytest.FakeMocks;

public class FakeServiceScope(IServiceProvider serviceProvider) : IServiceScope
{
	public IServiceProvider ServiceProvider { get; } = serviceProvider;

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
