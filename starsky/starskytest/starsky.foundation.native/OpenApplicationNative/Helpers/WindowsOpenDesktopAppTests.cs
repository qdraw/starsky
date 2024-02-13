using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class WindowsOpenDesktopAppTests
{
	[TestMethod]
	public void OpenDefault_HappyFlow()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var mock = new CreateFakeStarskyExe();
		var filePath = mock.FullFilePath;
		WindowsSetFileAssociations.EnsureAssociationsSet(
			new FileAssociation
			{
				Extension = ".starsky",
				ProgId = "starskytest",
				FileTypeDescription = "Starsky Test File",
				ExecutableFilePath = filePath
			});

		var result = WindowsOpenDesktopApp.OpenDefault(mock.StarskyDotStarskyPath);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void OpenDefault_FileNotFound()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var result = WindowsOpenDesktopApp.OpenDefault("not-found");
		Assert.IsFalse(result);
	}
}

