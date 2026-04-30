using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class TenantPathHelperTest
{
	[TestMethod]
	public void NormalizeForTenantScopedStorage_StripsTenantPrefix()
	{
		var result = TenantPathHelper.NormalizeForTenantScopedStorage("/main/0001/a.jpg", "main");
		Assert.AreEqual("/0001/a.jpg", result);
	}

	[TestMethod]
	public void NormalizeForTenantScopedStorage_ExactTenantRoot_ReturnsRoot()
	{
		var result = TenantPathHelper.NormalizeForTenantScopedStorage("/main", "main");
		Assert.AreEqual("/", result);
	}

	[TestMethod]
	public void ToTenantScopedPath_AddsPrefix_WhenMissing()
	{
		var result = TenantPathHelper.ToTenantScopedPath("/0001/a.jpg", "main");
		Assert.AreEqual("/main/0001/a.jpg", result);
	}

	[TestMethod]
	public void ToTenantScopedPath_DoesNotDuplicatePrefix()
	{
		var result = TenantPathHelper.ToTenantScopedPath("/main/0001/a.jpg", "main");
		Assert.AreEqual("/main/0001/a.jpg", result);
	}
}

