using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers;

[TestClass]
public class ShellThumbnailExtractionWindowsTest
{
	private static async Task<(string, string)> CreateTempImage(string testName,
		bool corrupt = false)
	{
		var tempFolder = new AppSettings().TempFolder;
		var tempInputPath = Path.Combine(tempFolder, $"{testName}_input.jpg");
		if ( !corrupt )
		{
			await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(
				new MemoryStream([.. new CreateAnImageWithThumbnail().Bytes]), tempInputPath);
		}
		else
		{
			await File.WriteAllTextAsync(tempInputPath, "corrupted data");
		}

		var tempOutputPath = Path.Combine(tempFolder, $"{testName}_output.bmp");
		return ( tempInputPath, tempOutputPath );
	}

	[TestMethod]
	public void GenerateThumbnail_UnsupportedPlatform_ReturnsFalse__OnlyOtherThanWindows()
	{
		if ( ShellThumbnailExtractionWindows.IsSupported() )
		{
			Assert.Inconclusive("This test is only valid on unsupported platforms.");
		}

		var result = ShellThumbnailExtractionWindows.GenerateThumbnail(
			"input.jpg", "output.bmp", 100, 100);

		Assert.IsFalse(result,
			"Expected GenerateThumbnail to return false on unsupported platforms.");
	}

	[TestMethod]
	public void GenerateThumbnailInternal_UnsupportedPlatform__OnlyOtherThanWindows()
	{
		if ( ShellThumbnailExtractionWindows.IsSupported() )
		{
			Assert.Inconclusive("This test is only valid on unsupported platforms.");
		}

		Assert.ThrowsExactly<MarshalDirectiveException>(() =>
		{
			ShellThumbnailExtractionWindows.GenerateThumbnailInternal(
				"input.jpg", "output.bmp", 100, 100);
		});
	}

	[TestMethod]
	public async Task GenerateThumbnail_ValidInput_CreatesThumbnail__OnlyOnWindowsX64()
	{
		if ( !ShellThumbnailExtractionWindows.IsSupported() )
		{
			Assert.Inconclusive("This test is only valid on Windows x64.");
		}

		// Arrange
		var (input, output) =
			await CreateTempImage("GenerateThumbnail_ValidInput_CreatesThumbnail");

		try
		{
			var result = ShellThumbnailExtractionWindows.GenerateThumbnail(
				input, output, 100, 67);

			Assert.IsTrue(result, "Expected GenerateThumbnail to return true.");
			Assert.IsTrue(File.Exists(output), "Output file was not created.");

			// test with imageSharp if the image is valid
			using var image = await Image.LoadAsync(output);
			Assert.IsNotNull(image, "Image should not be null.");
			Assert.AreEqual(100, image.Width, "Image width is not as expected.");
			Assert.AreEqual(67, image.Height, "Image height is not as expected.");
			Assert.IsTrue(new FileInfo(output).Length > 0, "Output file is empty.");
		}
		finally
		{
			File.Delete(input);
			if ( File.Exists(output) )
			{
				File.Delete(output);
			}
		}
	}

	[TestMethod]
	public async Task GenerateThumbnail_HBitmapIsZero()
	{
		var (input, output) =
			await CreateTempImage("GenerateThumbnail_HBitmapIsZero_ThrowsInvalidOperationException",
				true);

		// Mock or simulate the scenario where hBitmap is IntPtr.Zero
		// This can be done by providing invalid input or mocking the interop call
		var result = ShellThumbnailExtractionWindows.GenerateThumbnail(
			input, output, 100, 100);

		Assert.IsFalse(result, "Expected GenerateThumbnail to return false.");
	}

	[TestMethod]
	public void GenerateThumbnail_InvalidInput_ThrowsException__WindowsX64Only()
	{
		if ( !ShellThumbnailExtractionWindows.IsSupported() )
		{
			Assert.Inconclusive("This test is only valid on Windows x64.");
		}

		Assert.ThrowsExactly<FileNotFoundException>(() =>
		{
			ShellThumbnailExtractionWindows.GenerateThumbnail(
				"nonexistent.jpg", "output.bmp", 100, 100);
		});
	}

	[TestMethod]
	public void SaveHBitmapToBmp_InvalidOperationException__WindowsX64Only()
	{
		if ( !ShellThumbnailExtractionWindows.IsSupported() )
		{
			Assert.Inconclusive("This test is only valid on Windows x64.");
		}

		Assert.ThrowsExactly<InvalidOperationException>(() =>
		{
			ShellThumbnailExtractionWindows.SaveHBitmapToBmp(IntPtr.Zero,
				"test");
		});
	}

	[TestMethod]
	public async Task SaveHBitmapToBmp()
	{
		var (_, output) =
			await CreateTempImage("SaveHBitmapToBmp",
				true);

		ShellThumbnailExtractionWindows.SaveHBitmapToBmp(
			new ShellThumbnailExtractionWindows.BITMAP { bmBits = 1 },
			output);

		Assert.IsTrue(File.Exists(output), "Output file was not created.");
		Assert.IsTrue(new FileInfo(output).Length > 0, "Output file is empty.");

		File.Delete(output);
	}

	[TestMethod]
	public void SaveHBitmapToBmp__OnlyOtherThanWindows()
	{
		if ( ShellThumbnailExtractionWindows.IsSupported() )
		{
			Assert.Inconclusive("This test is only valid on unsupported platforms.");
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
		{
			ShellThumbnailExtractionWindows.SaveHBitmapToBmp(
				1,
				string.Empty);
		});
	}

	[TestMethod]
	public void WriteStruct_ArgumentException()
	{
		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			ShellThumbnailExtractionWindows.WriteStruct(null!,
				"test");
		});
	}

	[TestMethod]
	public void FlipImage_ShouldFlipImageVertically()
	{
		// Arrange
		var bmp = new ShellThumbnailExtractionWindows.BITMAP
		{
			bmWidth = 3, bmHeight = 2, bmWidthBytes = 3
		};

		// Pixel data for a 3x2 image (2 rows, 3 pixels each)
		// Row 1: [1, 2, 3]
		// Row 2: [4, 5, 6]
		var pixelBytes = new byte[] { 1, 2, 3, 4, 5, 6 };
		var expectedFlippedBytes = new byte[] { 4, 5, 6, 1, 2, 3 };

		// Act
		var flippedBytes =
			ShellThumbnailExtractionWindows.FlipImage(bmp, pixelBytes, pixelBytes.Length);

		// Assert
		CollectionAssert.AreEqual(expectedFlippedBytes, flippedBytes);
	}

	[TestMethod]
	public void SiIgbf()
	{
		Assert.AreEqual("SIIGBF_RESIZETOFIT",
			$"{ShellThumbnailExtractionWindows.SIIGBF.SIIGBF_RESIZETOFIT}");
		Assert.AreEqual("SIIGBF_BIGGERSIZEOK",
			$"{ShellThumbnailExtractionWindows.SIIGBF.SIIGBF_BIGGERSIZEOK}");
		Assert.AreEqual("SIIGBF_MEMORYONLY",
			$"{ShellThumbnailExtractionWindows.SIIGBF.SIIGBF_MEMORYONLY}");
		Assert.AreEqual("SIIGBF_ICONONLY",
			$"{ShellThumbnailExtractionWindows.SIIGBF.SIIGBF_ICONONLY}");
		Assert.AreEqual("SIIGBF_THUMBNAILONLY",
			$"{ShellThumbnailExtractionWindows.SIIGBF.SIIGBF_THUMBNAILONLY}");
		Assert.AreEqual("SIIGBF_INCACHEONLY",
			$"{ShellThumbnailExtractionWindows.SIIGBF.SIIGBF_INCACHEONLY}");
	}
}
