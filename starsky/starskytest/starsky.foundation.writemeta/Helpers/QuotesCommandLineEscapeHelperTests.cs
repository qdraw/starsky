using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.writemeta.Helpers;

namespace starskytest.starsky.foundation.writemeta.Helpers;

[TestClass]
public class QuotesCommandLineEscapeHelperTests
{
	[TestMethod]
	public void QuotesCommandLineEscape_EmptyString()
	{
		var input = string.Empty;
		var result = input.QuotesCommandLineEscape();
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void QuotesCommandLineEscape_Quoted()
	{
		const string input = "\"";
		const string expectedOutput = "\\\"";

		var result = input.QuotesCommandLineEscape();
		Assert.AreEqual(expectedOutput, result);
	}

	[TestMethod]
	public void QuotesCommandLineEscape_Null()
	{
		const string? input = null;

		var result = input.QuotesCommandLineEscape();
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	[DataRow("a\\b", "a\\b")] // lone backslash stays
	[DataRow("a\\", "a\\\\")] // trailing backslash is doubled, it is in front of the closing quote
	[DataRow("\\\"", "\\\\\\\"")] // \" => \\\"
	[DataRow("a\"b", "a\\\"b")]
	public void QuotesCommandLineEscape_Backslashes(string input, string expected)
	{
		Assert.AreEqual(expected, input.QuotesCommandLineEscape());
	}
}
