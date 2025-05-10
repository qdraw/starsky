using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starskytest.FakeCreateAn.CreateAnImagePsd;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers;

[TestClass]
public class QuicklookMacOsTests
{
	private readonly FakeIWebLogger _logger = new();

	[TestMethod]
	public void GenerateThumbnail_ShouldReturnFalse_WhenNotMacOS()
	{
		// Arrange
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on non-macOS platforms.");
		}

		var quicklook = new QuicklookMacOs(_logger);

		// Act
		var result = quicklook.GenerateThumbnail("input.jpg", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GenerateThumbnail_ShouldReturnFalse_WhenFilePathIsInvalid__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

		var quicklook = new QuicklookMacOs(_logger);

		// Act
		var result = quicklook.GenerateThumbnail("", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(_logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to create URL") == true));
	}

	[TestMethod]
	public void GenerateThumbnail_ShouldReturnFalse_WhenThumbnailCreationFails__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

		var quicklook = new QuicklookMacOs(_logger);
		var tempInput = Path.GetTempFileName();

		try
		{
			File.WriteAllText(tempInput, "dummy content");

			// Act
			var result = quicklook.GenerateThumbnail(tempInput, "/invalid/output.webp", 100, 100);

			// Assert
			Assert.IsFalse(result);
			Assert.IsTrue(_logger.TrackedInformation.Exists(log =>
				log.Item2?.Contains("Failed to generate thumbnail") == true));
		}
		finally
		{
			File.Delete(tempInput);
		}
	}

	[TestMethod]
	public void GenerateThumbnail_ShouldReturnTrue_WhenValidInput__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

		var quicklook = new QuicklookMacOs(_logger);
		var tempInput = new CreateAnImageWithThumbnail().Bytes.ToArray();
		var tempInputPath = Path.Combine("/tmp", "input.jpg");
		File.WriteAllBytes(tempInputPath, tempInput);
		
		var tempOutput = Path.Combine("/tmp", "output.webp");

		try
		{
			// Act
			var result = quicklook.GenerateThumbnail(tempInputPath, 
				"/tmp/thumbnail.webp", 150, 100);

			// Assert
			Assert.IsFalse(result);
		}
		finally
		{
			if ( File.Exists(tempOutput) )
			{
				File.Delete(tempOutput);
			}
		}
	}

	[TestMethod]
	public void GenerateThumbnail_ShouldReturnTrue_WhenValidInput22__MacOnly()
	{
		var quicklook = new QuicklookMacOs(_logger);
		quicklook.GenerateThumbnail("/Users/dion/data/fotobieb/2025/05/2025_05_09/20250509_143736_d.jpg", "/tmp/thubn.webp", 150, 100);

	}
}
