using System;
using System.Diagnostics.Metrics;

namespace starskytest.FakeMocks;

public sealed class FakeIMeterFactory : IMeterFactory
{
	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public Meter Create(MeterOptions options)
	{
		return new Meter(options);
	}
}
