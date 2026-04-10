using System;
using System.IO;
using System.Linq;
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
	[OSCondition(ConditionMode.Exclude, OperatingSystems.OSX)]
	public void GenerateThumbnail_False__WindowsLinuxOnly()
	{
		var (quicklook, _) = CreateSut();
		// Act
		Assert.IsFalse(quicklook.GenerateThumbnail("input.jpg", "output.webp", 100, 100));
	}

	private static (QuicklookMacOs, FakeIWebLogger) CreateSut()
	{
		var logger = new FakeIWebLogger();
		var quicklook = new QuicklookMacOs(logger);
		return ( quicklook, logger );
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void GenerateThumbnail_ShouldReturnFalse_WhenFilePathIsInvalid__MacOnly()
	{
		var (quicklook, logger) = CreateSut();

		// Act
		var result = quicklook.GenerateThumbnail("", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to create URL") == true));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void GenerateThumbnail_ShouldReturnFalse_WhenThumbnailCreationFails__MacOnly()
	{
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

	/// <summary>
	///     Happy flow
	/// </summary>
	[TestMethod]
	[DataRow(0)]
	[DataRow(100)]
	[OSCondition(OperatingSystems.OSX)]
	public void GenerateThumbnail_ShouldReturnTrue_WhenValidInput_Jpeg__MacOnly(int height)
	{
		const string testName =
			nameof(GenerateThumbnail_ShouldReturnTrue_WhenValidInput_Jpeg__MacOnly);
		var (quicklook, _) = CreateSut();

		var tempInput = new CreateAnImageWithThumbnail().Bytes.ToArray();
		var tempFolder = new AppSettings().TempFolder;
		var tempInputPath = Path.Combine(tempFolder, $"{testName}{height}_input.jpg");
		File.WriteAllBytes(tempInputPath, tempInput);

		var tempOutput = Path.Combine(tempFolder, $"{testName}{height}_output.jpg");

		try
		{
			// Act
			var result = quicklook.GenerateThumbnail(tempInputPath,
				tempOutput, 150, height);

			// Assert
			Assert.IsTrue(result);
			Assert.IsTrue(File.Exists(tempOutput));
			Assert.IsGreaterThan(0, new FileInfo(tempOutput).Length);
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
	[OSCondition(OperatingSystems.OSX)]
	public void SaveCGImageAsFile_InvalidType__MacOnly()
	{
		var (quicklook, logger) = CreateSut();

		// Act
		var result = quicklook.SaveCGImageAsFile(1, "output.webp", "invalid_type");
		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to create image destination") == true));
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.OSX)]
	public void SaveCGImageAsFile_DllNotFoundException__WindowsLinuxOnly()
	{
		var (quicklook, _) = CreateSut();

		// Act
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			quicklook.SaveCGImageAsFile(1, "output.webp"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void ImageDestinationAddImageFinalize_InvalidType__MacOnly()
	{
		var (quicklook, logger) = CreateSut();

		// Act
		quicklook.ImageDestinationAddImageFinalize(IntPtr.Zero, IntPtr.Zero);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to finalize image") == true));
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.OSX)]
	public void ImageDestinationAddImageFinalize_DllNotFoundException__WindowsLinuxOnly()
	{
		var (quicklook, _) = CreateSut();

		// Act
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			quicklook.ImageDestinationAddImageFinalize(1, IntPtr.Zero));
	}
}
