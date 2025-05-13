using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers;

[TestClass]
public class QuicklookMacOsTests
{
	[TestMethod]
	public void GenerateThumbnail_DllNotFoundException__WindowsLinuxOnly()
	{
		// Arrange
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on non-macOS platforms.");
		}

		var (quicklook, _) = CreateSut();
		// Act
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			quicklook.GenerateThumbnail("input.jpg", "output.webp", 100, 100));
	}

	private static (QuicklookMacOs, FakeIWebLogger) CreateSut()
	{
		var logger = new FakeIWebLogger();
		var quicklook = new QuicklookMacOs(logger);
		return ( quicklook, logger );
	}

	[TestMethod]
	public void GenerateThumbnail_ShouldReturnFalse_WhenFilePathIsInvalid__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

		var (quicklook, logger) = CreateSut();

		// Act
		var result = quicklook.GenerateThumbnail("", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
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

		var (quicklook, logger) = CreateSut();

		var tempInput = Path.GetTempFileName();

		try
		{
			File.WriteAllText(tempInput, "dummy content");

			// Act
			var result = quicklook.GenerateThumbnail(tempInput, "/invalid/output.webp", 100, 100);

			// Assert
			Assert.IsFalse(result);
			Assert.IsTrue(logger.TrackedInformation.Exists(log =>
				log.Item2?.Contains("Failed to generate thumbnail") == true));
		}
		finally
		{
			File.Delete(tempInput);
		}
	}

	[TestMethod]
	public void GenerateThumbnail_ShouldReturnTrue_WhenValidInput_Jpeg__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

		const string testName =
			nameof(GenerateThumbnail_ShouldReturnTrue_WhenValidInput_Jpeg__MacOnly);
		var (quicklook, _) = CreateSut();

		var tempInput = new CreateAnImageWithThumbnail().Bytes.ToArray();
		var tempFolder = new AppSettings().TempFolder;
		var tempInputPath = Path.Combine(tempFolder, $"{testName}_input.jpg");
		File.WriteAllBytes(tempInputPath, tempInput);

		var tempOutput = Path.Combine(tempFolder, $"{testName}_output.jpg");

		try
		{
			// Act
			var result = quicklook.GenerateThumbnail(tempInputPath,
				tempOutput, 150, 100);

			// Assert
			Assert.IsTrue(result);
			Assert.IsTrue(File.Exists(tempOutput));
			Assert.IsTrue(new FileInfo(tempOutput).Length > 0);
			// detect with imageSharp if the image is a valid image
			using var image = Image.Load(tempOutput);
			Assert.IsNotNull(image);
			Assert.AreEqual(150, image.Width);
			Assert.AreEqual(100, image.Height);
		}
		finally
		{
			File.Delete(tempInputPath);
			if ( File.Exists(tempOutput) )
			{
				File.Delete(tempOutput);
			}
		}
	}

	[TestMethod]
	public void SaveCGImageAsFile_InvalidType__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

		var (quicklook, logger) = CreateSut();

		// Act
		var result = quicklook.SaveCGImageAsFile(1, "output.webp", "invalid_type");
		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to create image destination") == true));
	}

	[TestMethod]
	public void SaveCGImageAsFile_DllNotFoundException__WindowsLinuxOnly()
	{
		// Arrange
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on non-macOS platforms.");
		}

		var (quicklook, _) = CreateSut();

		// Act
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			quicklook.SaveCGImageAsFile(1, "output.webp"));
	}

	[TestMethod]
	public void ImageDestinationAddImageFinalize_InvalidType__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

		var (quicklook, logger) = CreateSut();

		// Act
		quicklook.ImageDestinationAddImageFinalize(IntPtr.Zero, IntPtr.Zero);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to finalize image") == true));
	}

	[TestMethod]
	public void ImageDestinationAddImageFinalize_DllNotFoundException__WindowsLinuxOnly()
	{
		// Arrange
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on non-macOS platforms.");
		}

		var (quicklook, _) = CreateSut();

		// Act
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			quicklook.ImageDestinationAddImageFinalize(1, IntPtr.Zero));
	}

	[TestMethod]
	public void CreateCFStringCreateWithCString_DllNotFoundException__WindowsLinuxOnly()
	{
		// Arrange
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on non-macOS platforms.");
		}

		// Act
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			QuicklookMacOs.CreateCFStringCreateWithCString("input.jpg"));
	}
}
