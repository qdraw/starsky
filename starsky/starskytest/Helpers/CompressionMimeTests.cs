using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;

namespace starskytest.Helpers;

[TestClass]
public class CompressionMimeTests
{
	[TestMethod]
	public void GetCompressionMimeTypes_ShouldCombineDefaultAndCustomMimeTypes()
	{
		// Act
		var result = CompressionMime.GetCompressionMimeTypes().ToList();

		// Assert
		Assert.IsTrue(result.Contains("application/xhtml+xml"));
		Assert.IsTrue(result.Contains("image/svg+xml"));
		Assert.IsTrue(
			result.Contains("text/plain")); // Example from ResponseCompressionDefaults.MimeTypes
	}
}
