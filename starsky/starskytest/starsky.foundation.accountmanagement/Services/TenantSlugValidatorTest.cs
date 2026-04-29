using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Services;

namespace starskytest.starsky.foundation.accountmanagement.Services;

[TestClass]
public class TenantSlugValidatorTest
{
	[TestMethod]
	[DataRow("main")]
	[DataRow("abc-123")]
	[DataRow("a1b")]
	[DataRow("tenant-01")]
	public void IsValid_ValidSlug_ReturnsTrue(string slug)
	{
		var sut = new TenantSlugValidator();
		Assert.IsTrue(sut.IsValid(slug));
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("A")]
	[DataRow("a")]
	[DataRow("a1")]
	[DataRow("-abc")]
	[DataRow("abc-")]
	[DataRow("abc_def")]
	[DataRow("with space")]
	public void IsValid_InvalidSlug_ReturnsFalse(string slug)
	{
		var sut = new TenantSlugValidator();
		Assert.IsFalse(sut.IsValid(slug));
	}
}
