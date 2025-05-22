using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers;

[TestClass]
public class ImageIoMacOsBindingsTests
{
	[DataTestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public void GetSourceHeight_ValidImage_ReturnsHeight__MacOnly(bool isValidImage)
	{
		if ( !IsMacOs() )
		{
			Assert.Inconclusive("Test only runs on macOS.");
		}

		var imagePath =
			isValidImage ? new CreateAnImage().FullFilePath : "/path/to/invalid-image.jpg";
		var cfStringUrl = CoreFoundationMacOsBindings.CreateCFStringCreateWithCString(imagePath);

		var height = ImageIoMacOsBindings.GetSourceHeight(cfStringUrl);

		Assert.AreEqual(isValidImage ? 2 : 0, height);
	}

	[TestMethod]
	public void GetSourceHeight__WindowsLinuxOnly()
	{
		if ( IsMacOs() )
		{
			Assert.Inconclusive("Test only runs on Windows or Linux.");
		}

		Assert.ThrowsExactly<DllNotFoundException>(() =>
			ImageIoMacOsBindings.GetSourceHeight(0));
	}

	private static bool IsMacOs()
	{
		return Environment.OSVersion.Platform == PlatformID.MacOSX ||
		       ( Environment.OSVersion.Platform == PlatformID.Unix &&
		         Directory.Exists("/System/Library/Frameworks") );
	}
}
