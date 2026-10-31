using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Tenancy;

namespace starskytest.starsky.foundation.platform.Tenancy;

[TestClass]
public sealed class TenantIdHelperTest
{
	[TestMethod]
	[DataRow("default", true)]
	[DataRow("acme", true)]
	[DataRow("acme-2", true)]
	[DataRow("", false)]
	[DataRow(null, false)]
	[DataRow("Acme", false)]
	[DataRow("..", false)]
	[DataRow("a/b", false)]
	[DataRow("a\\b", false)]
	[DataRow("-acme", false)]
	[DataRow("acme-", false)]
	[DataRow("acme--x", false)]
	[DataRow("a.b", false)]
	public void IsValid(string? tenantId, bool expected)
	{
		Assert.AreEqual(expected, TenantIdHelper.IsValid(tenantId));
	}

	[TestMethod]
	public void IsValid_TooLong()
	{
		Assert.IsFalse(TenantIdHelper.IsValid(new string('a', 64)));
		Assert.IsTrue(TenantIdHelper.IsValid(new string('a', 63)));
	}
}
