using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class WindowsOpenDesktopAppTests
{

	private const string Extension = ".starsky";
	private const string ProgId = "starskytest";
	private const string FileTypeDescription = "Starsky Test File";


	[TestInitialize]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", 
		"CA1416:Validate platform compatibility", Justification = "Check does exists")]
	public void TestInitialize()
	{
		if ( !new AppSettings().IsWindows )
		{
			return;
		}

		// Ensure no keys exist before the test starts
		Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{Extension}", false);
		Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgId}", false);
	}

	[TestCleanup]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
		"CA1416:Validate platform compatibility", Justification = "Check does exists")]
	public void TestCleanup()
	{
		if ( !new AppSettings().IsWindows )
		{
			return;
		}

		// Cleanup created keys
		Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{Extension}", false);
		Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgId}", false);
	}

	[TestMethod]
	public void W_OpenDefault_NonWindows()
	{
		var result = WindowsOpenDesktopApp.OpenDefault(["any value"], OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void w_OpenDefault_HappyFlow__WindowsOnly()
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
				Extension = Extension,
				ProgId = ProgId,
				FileTypeDescription = FileTypeDescription,
				ExecutableFilePath = filePath
			});

		var result = WindowsOpenDesktopApp.OpenDefault([mock.StarskyDotStarskyPath], OSPlatform.Windows);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void W_OpenApplicationAtUrl_NonWindows()
	{
		var result = WindowsOpenDesktopApp.OpenApplicationAtUrl(new List<string> { "any value" }, "app", OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void W_OpenApplicationAtUrl_ReturnsTrue_WhenApplicationOpens()
	{
		// Arrange
		var mock = new CreateFakeStarskyExe();

		var fileUrls = new List<string>
			{
				mock.StarskyDotStarskyPath,
			};

		// @"C:\Windows\System32\notepad.exe"; // Example application URL (notepad.exe)

																 // Act
		var result = WindowsOpenDesktopApp.OpenApplicationAtUrl(fileUrls, mock.FullFilePath);

		// Assert
		Assert.IsTrue(result);
	}


	[TestMethod]
	public void W_OpenDefault_FileNotFound__WindowsOnly()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var result = WindowsOpenDesktopApp.OpenDefault(["C:\\not-found-74537587345853847345"], OSPlatform.Windows);
		Assert.IsFalse(result);
	}
}

