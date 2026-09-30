using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
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
	///     Repro/regression test for the native CoreGraphics/ImageIO leak: IsImageWhite created a
	///     CFString, CFURL, CGImageSource, CGImage, CGColorSpace and CGBitmapContext (with a real
	///     width*height*4-byte pixel buffer) per call but never released any of them.
	///
	///     A single before/after memory measurement is too noisy for this: even correctly
	///     released CoreGraphics/ImageIO objects become "purgeable" rather than being returned to
	///     the OS immediately, so resident memory fluctuates run to run before the OS reclaims it.
	///     The reliable signature (verified against both the original bug and the fix) is
	///     different: a genuinely leaked (never-released, refcount never zero) object can NEVER
	///     be reclaimed, so repeated measurements only ever climb - confirmed to grow strictly and
	///     steadily (e.g. 447MB -> 888MB -> 1227MB -> 1758MB -> 2196MB -> 2714MB over 60 calls) on
	///     the original bug. Correctly released memory, being purgeable, gets reclaimed by the OS
	///     at some point during the run, so at least one checkpoint drops back near baseline
	///     (observed dropping to ~3MB) - something that is structurally impossible for a true leak.
	///     This test therefore takes several checkpoints over many calls and asserts the *minimum*
	///     observed growth is small, rather than asserting on a single, noisy endpoint.
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
		SettleNativeMemory();

		using var process = Process.GetCurrentProcess();
		process.Refresh();
		var before = process.WorkingSet64;

		const int iterations = 60;
		const int checkpointEvery = 10;
		var minGrowthAcrossCheckpoints = long.MaxValue;

		for ( var i = 1; i <= iterations; i++ )
		{
			WhiteImageDetectorMacOsBindings.IsImageWhite(imagePath);

			if ( i % checkpointEvery != 0 )
			{
				continue;
			}

			SettleNativeMemory();
			process.Refresh();
			var grownBytes = process.WorkingSet64 - before;
			minGrowthAcrossCheckpoints = Math.Min(minGrowthAcrossCheckpoints, grownBytes);
		}

		// The original bug grew strictly monotonically by ~45MB per call with no exceptions,
		// so its lowest checkpoint after 60 calls would still be >1GB. 300MB comfortably clears
		// the observed reclaimed-but-not-yet-fully-settled noise while staying far below that.
		const long maxAllowedMinGrowthBytes = 300L * 1024 * 1024;
		Assert.IsLessThan(maxAllowedMinGrowthBytes, minGrowthAcrossCheckpoints,
			"Resident memory never dropped back down across checkpoints (lowest observed " +
			$"growth was {minGrowthAcrossCheckpoints / 1024 / 1024}MB over {iterations} calls) " +
			"- native CoreGraphics/ImageIO objects are being leaked (not released), since " +
			"correctly-released purgeable memory would have been reclaimed by the OS at least once");
	}

	private static void SettleNativeMemory()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		malloc_zone_pressure_relief(IntPtr.Zero, UIntPtr.Zero);
	}

	[SuppressMessage("Interoperability",
		"SYSLIB1054:Use 'LibraryImportAttribute' instead of " +
		"'DllImportAttribute' to generate P/Invoke marshalling code at compile time")]
	[DllImport("/usr/lib/system/libsystem_malloc.dylib")]
	private static extern UIntPtr malloc_zone_pressure_relief(IntPtr zone, UIntPtr goal);

	private static bool IsMacOs()
	{
		return Environment.OSVersion.Platform == PlatformID.MacOSX ||
		       ( Environment.OSVersion.Platform == PlatformID.Unix &&
		         Directory.Exists("/System/Library/Frameworks") );
	}
}
