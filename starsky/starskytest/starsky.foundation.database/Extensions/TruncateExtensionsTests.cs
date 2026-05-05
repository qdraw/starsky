using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Extensions;

namespace starskytest.starsky.foundation.database.Extensions;

[TestClass]
public class TruncateExtensionsTests
{
	[TestMethod]
	public void TruncateWithEllipsis_ShortMaxLength_NoEllipsis()
	{
		const string input = "abcdef";
		var result = input.TruncateWithEllipsis(2); // maxLength < 3 branch
		Assert.AreEqual("ab", result);
	}

	[TestMethod]
	public void TruncateWithEllipsis_EmptyString()
	{
		var input = string.Empty;
		var result = input.TruncateWithEllipsis(50);
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void TruncateWithEllipsis_ShortMaxLength_Exact()
	{
		var input = "ab";
		var result = input.TruncateWithEllipsis(2);
		Assert.AreEqual("ab", result);
	}

	[TestMethod]
	public void TruncateWithEllipsis_MaxLengthThree_ReturnsEllipsisWhenTooLong()
	{
		const string input = "abcdef";
		var result = input.TruncateWithEllipsis(3);
		Assert.AreEqual("...", result);
	}

	[TestMethod]
	public void TruncateWithEllipsis_NormalTruncate_AddsEllipsis()
	{
		const string input = "HelloWorld";
		var result = input.TruncateWithEllipsis(8);
		// maxLength 8 -> keep 5 chars + "..."
		Assert.AreEqual("Hello...", result);
	}

	[TestMethod]
	public void TruncateWithEllipsis_NoTruncate_WhenShorter()
	{
		const string input = "Hi";
		var result = input.TruncateWithEllipsis(10);
		Assert.AreEqual("Hi", result);
	}
}
