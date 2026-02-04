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
		Assert.Contains("application/xhtml+xml", result);
		Assert.Contains("image/svg+xml", result);
		Assert.Contains("text/plain", result); // Example from ResponseCompressionDefaults.MimeTypes
	}
}
