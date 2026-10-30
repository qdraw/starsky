using System;

namespace starskytest.FakeMocks;

public sealed class FakeTimeProvider : TimeProvider
{
	public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

	public override DateTimeOffset GetUtcNow()
	{
		return Now;
	}
}
