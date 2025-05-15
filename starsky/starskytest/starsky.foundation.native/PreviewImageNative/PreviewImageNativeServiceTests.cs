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
		Assert.AreEqual(0, logger.TrackedInformation.Count); // No logs expected
	}

	[TestMethod]
	public void GeneratePreviewImage_ShouldReturnFalse_WhenThumbnailGenerationFails__MacOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on macOS platforms.");
		}

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
	public void GeneratePreviewImage_ShouldReturnFalse_WhenThumbnailGenerationFails__WindowsOnly()
	{
		// Arrange
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			Assert.Inconclusive("This test is only valid on Windows platforms.");
		}

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
	public void GeneratePreviewImage_False__WindowsLinuxOnly()
	{
		// Arrange
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) )
		{
			Assert.Inconclusive("This test is only valid on non-macOS platforms.");
		}

		var (service, _) = CreateSut();

		// Act
		var result = service.GeneratePreviewImage(
			os => os == OSPlatform.OSX, // Simulate macOS
			"", "output.webp", 100, 100);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsSupported__MacAndWindowsOnly()
	{
		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Linux) )
		{
			Assert.Inconclusive("This test is only valid on windows / mac os platforms.");
		}

		var (service, _) = CreateSut();
		var big = service.IsSupported(1024);
		Assert.AreEqual(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows), big);
	}

	[TestMethod]
	public void IsSupported__LinuxOnly()
	{
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Linux) )
		{
			Assert.Inconclusive("This test is only valid on LInux platforms.");
		}

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
