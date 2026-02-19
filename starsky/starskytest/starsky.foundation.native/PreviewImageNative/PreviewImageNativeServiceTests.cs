using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.native.PreviewImageNative;

[TestClass]
public class PreviewImageNativeServiceTests
{
	private static (PreviewImageNativeService, FakeIWebLogger) CreateSut()
	{
		var logger = new FakeIWebLogger();
		var service = new PreviewImageNativeService(logger);
		return ( service, logger );
	}

	[TestMethod]
	public void GeneratePreviewImage_DefaultOption_ShouldReturnFalse_WhenNotMacOSorWindows()
	{
		// Arrange
		var (service, _) = CreateSut();

		// Act
		var result = service.GeneratePreviewImage("input.jpg", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GeneratePreviewImage_ShouldReturnFalse_WhenNotMacOSorWindows()
	{
		// Arrange
		var (service, logger) = CreateSut();

		// Act
		var result = service.GeneratePreviewImage(
			os => os == OSPlatform.Linux, // Simulate non-macOS/windows
			"input.jpg", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
		Assert.IsEmpty(logger.TrackedInformation); // No logs expected
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void GeneratePreviewImage_ShouldReturnFalse_WhenThumbnailGenerationFails__MacOnly()
	{
		var (service, logger) = CreateSut();

		// Act
		var result = service.GeneratePreviewImage(
			os => os == OSPlatform.OSX, // Simulate macOS
			"", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to create URL") == true));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Windows)]
	public void GeneratePreviewImage_ShouldReturnFalse_WhenThumbnailGenerationFails__WindowsOnly()
	{
		var (service, logger) = CreateSut();

		// Act
		var result = service.GeneratePreviewImage(
			os => os == OSPlatform.Windows,
			"", "output.bmp", 100, 100);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedInformation.Exists(log =>
			log.Item2?.Contains("Failed to create URL") == true));
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.OSX)]
	public void GeneratePreviewImage_False__WindowsLinuxOnly()
	{
		var (service, _) = CreateSut();

		// Act
		var result = service.GeneratePreviewImage(
			os => os == OSPlatform.OSX, // Simulate macOS
			"", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
	public void GeneratePreviewImage_False__MacLinuxOnly()
	{
		var (service, _) = CreateSut();

		// Act
		var result = service.GeneratePreviewImage(
			os => os == OSPlatform.Windows, // Simulate windows
			"", "output.bmp", 100, 100);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.Linux)]
	public void IsSupported__MacAndWindowsOnly()
	{
		var (service, _) = CreateSut();
		var big = service.IsSupported(1024);
		Assert.AreEqual(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows), big);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux)]
	public void IsSupported__LinuxOnly()
	{
		var (service, _) = CreateSut();
		var result = service.IsSupported();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void FileExtensionTest()
	{
		var ext = "jpg";
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			ext = "bmp";
		}

		var (service, _) = CreateSut();

		var result = service.FileExtension();
		Assert.AreEqual(ext, result);
	}
}
