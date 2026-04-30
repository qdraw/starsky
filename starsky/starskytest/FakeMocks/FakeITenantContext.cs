using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks;

public sealed class FakeITenantContext : ITenantContext
{
	public int? TenantId { get; set; }
	public string? TenantSlug { get; set; }
}

