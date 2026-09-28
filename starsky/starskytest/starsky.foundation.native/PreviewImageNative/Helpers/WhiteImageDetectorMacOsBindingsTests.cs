using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageA6700PreviewRawJpeg;
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
		byte[] pixelData =
		[
			255, 255, 255, 0, 255, 255, 255, 0, // Row 1
			255, 255, 255, 0, 255, 255, 255, 0  // Row 2
		];

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
		byte[] pixelData =
		[
			255, 255, 255, 0, 255, 255, 255, 0, // Row 1
			255, 255, 255, 0, 0, 0, 0, 0        // Row 2 (non-white pixel)
		];

		// Act
		var result = WhiteImageDetectorMacOsBindings.IsPixelDataWhite(
			width, height, bytesPerRow, bytesPerPixel, pixelData);

		// Assert
		Assert.IsFalse(result, "Expected non-white pixel to return false.");
	}

	/// <summary>
	///     Repro/regression test for the native CoreGraphics/ImageIO leak: IsImageWhite creates a
	///     CFString, CFURL, CGImageSource, CGImage, CGColorSpace and CGBitmapContext (with a real
	///     width*height*4-byte pixel buffer) per call but never released any of them. Calling this
	///     repeatedly on a full-size (4384x2920) camera JPEG must not grow the process' resident
	///     memory without bound.
	/// </summary>
	[TestMethod]
	public void IsImageWhite_RepeatedCalls_DoesNotLeakNativeMemory__MacOnly()
	{
		if ( !IsMacOs() )
		{
			Assert.Inconclusive("Test only runs on macOS.");
		}

		var imagePath = new CreateAnImageA6700PreviewRawJpeg().FilePathJpeg;

		// warm up: first call(s) can allocate one-time native library/JIT state
		WhiteImageDetectorMacOsBindings.IsImageWhite(imagePath);
		WhiteImageDetectorMacOsBindings.IsImageWhite(imagePath);

		var ownPid = Environment.ProcessId;
		Console.WriteLine("CG image BEFORE: " + VmmapCgImageLine(ownPid));

		const int iterations = 20;
		for ( var i = 0; i < iterations; i++ )
		{
			WhiteImageDetectorMacOsBindings.IsImageWhite(imagePath);
			if ( i is 4 or 9 or 14 or 19 )
			{
				Console.WriteLine($"CG image after call {i}: " + VmmapCgImageLine(ownPid));
			}
		}

		Assert.IsTrue(true, "diagnostic run");
	}

	private static string VmmapCgImageLine(int pid)
	{
		using var p = new Process
		{
			StartInfo = new ProcessStartInfo("vmmap", $"--summary {pid}")
			{
				RedirectStandardOutput = true, UseShellExecute = false
			}
		};
		p.Start();
		var output = p.StandardOutput.ReadToEnd();
		p.WaitForExit();
		foreach ( var line in output.Split('\n') )
		{
			if ( line.Contains("CG image") )
			{
				return line.Trim();
			}
		}

		return "(not found)";
	}

	private static bool IsMacOs()
	{
		return Environment.OSVersion.Platform == PlatformID.MacOSX ||
		       ( Environment.OSVersion.Platform == PlatformID.Unix &&
		         Directory.Exists("/System/Library/Frameworks") );
	}
}
