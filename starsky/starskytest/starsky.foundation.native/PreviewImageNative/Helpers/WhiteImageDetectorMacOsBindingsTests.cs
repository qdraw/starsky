using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageWhiteJpeg;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers;

[TestClass]
public class WhiteImageDetectorMacOsBindingsTests
{
	[TestMethod]
	public void IsImageWhite_WhiteImage_ReturnsTrue__MacOnly()
	{
		if ( !IsMacOs() )
		{
			Assert.Inconclusive("Test only runs on macOS.");
		}

		var imagePath = new CreateAnImageWhiteJpeg().FullFilePath;
		var result = WhiteImageDetectorMacOsBindings.IsImageWhite(imagePath);

		Assert.IsTrue(result, "Expected the image to be detected as white.");
	}

	[TestMethod]
	public void IsImageWhite_NonWhiteImage_ReturnsFalse__MacOnly()
	{
		if ( !IsMacOs() )
		{
			Assert.Inconclusive("Test only runs on macOS.");
		}

		var imagePath = new CreateAnImage().FullFilePath;
		var result = WhiteImageDetectorMacOsBindings.IsImageWhite(imagePath);

		Assert.IsFalse(result, "Expected the image to be detected as non-white.");
	}

	[TestMethod]
	public void IsImageWhite_InvalidPath_ReturnsFalse__MacOnly()
	{
		if ( !IsMacOs() )
		{
			Assert.Inconclusive("Test only runs on macOS.");
		}

		var invalidPath = "/path/to/nonexistent-image.jpg";
		var result = WhiteImageDetectorMacOsBindings.IsImageWhite(invalidPath);

		Assert.IsFalse(result, "Expected the method to return false for an invalid path.");
	}

	[TestMethod]
	public void IsImageWhite_WhiteImage_ReturnsTrue__WindowsLinuxOnly()
	{
		if ( IsMacOs() )
		{
			Assert.Inconclusive("Test only runs on Windows/Linux");
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			WhiteImageDetectorMacOsBindings.IsImageWhite("anything"));
	}
	
	[TestMethod]
	public void IsPixelDataWhite_AllWhitePixels_ReturnsTrue()
	{
		// Arrange
		const int width = 2;
		const int height = 2;
		const int bytesPerRow = 8;
		const int bytesPerPixel = 4;
		byte[] pixelData = {
			255, 255, 255, 0, 255, 255, 255, 0, // Row 1
			255, 255, 255, 0, 255, 255, 255, 0  // Row 2
		};

		// Act
		var result = WhiteImageDetectorMacOsBindings.IsPixelDataWhite(
			width, height, bytesPerRow, bytesPerPixel, pixelData);

		// Assert
		Assert.IsTrue(result, "Expected all-white pixels to return true.");
	}

	[TestMethod]
	public void IsPixelDataWhite_NonWhitePixel_ReturnsFalse()
	{
		// Arrange
		const int width = 2;
		const int height = 2;
		const int bytesPerRow = 8;
		const int bytesPerPixel = 4;
		byte[] pixelData = {
			255, 255, 255, 0, 255, 255, 255, 0, // Row 1
			255, 255, 255, 0, 0, 0, 0, 0        // Row 2 (non-white pixel)
		};

		// Act
		var result = WhiteImageDetectorMacOsBindings.IsPixelDataWhite(
			width, height, bytesPerRow, bytesPerPixel, pixelData);

		// Assert
		Assert.IsFalse(result, "Expected non-white pixel to return false.");
	}

	private static bool IsMacOs()
	{
		return Environment.OSVersion.Platform == PlatformID.MacOSX ||
		       ( Environment.OSVersion.Platform == PlatformID.Unix &&
		         Directory.Exists("/System/Library/Frameworks") );
	}
}
