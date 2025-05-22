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
}
