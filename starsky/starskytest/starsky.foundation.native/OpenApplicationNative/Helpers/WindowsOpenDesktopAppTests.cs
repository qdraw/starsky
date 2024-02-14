using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Medallion.Shell;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class WindowsOpenDesktopAppTests
{
	private const string Extension = ".starsky";
	private const string ProgId = "starskytest";
	private const string FileTypeDescription = "Starsky Test File";

	[TestInitialize]
	public void TestInitialize()
	{
		CleanSetup();
	}

	[TestCleanup]
	public void TestCleanup()
	{
		CleanSetup();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
		"CA1416:Validate platform compatibility", Justification = "Check does exists")]
	private static void CleanSetup()
	{
		if ( !new AppSettings().IsWindows )
		{
			return;
		}

		// Ensure no keys exist before the test starts
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
	public void W_OpenDefault_HappyFlow__WindowsOnly()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var mock = new CreateFakeStarskyWindowsExe();
		var filePath = mock.FullFilePath;
		WindowsSetFileAssociations.EnsureAssociationsSet(
			new FileAssociation
			{
				Extension = Extension,
				ProgId = ProgId,
				FileTypeDescription = FileTypeDescription,
				ExecutableFilePath = filePath
			});

		var result =
			WindowsOpenDesktopApp.OpenDefault([mock.StarskyDotStarskyPath], OSPlatform.Windows);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void W_OpenApplicationAtUrl_NonWindows()
	{
		var result = WindowsOpenDesktopApp.OpenApplicationAtUrl(new List<string> { "any value" },
			"app", OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void W_OpenApplicationAtUrl_ReturnsTrue_WhenApplicationOpens__WindowsOnly()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		// Arrange
		var mock = new CreateFakeStarskyWindowsExe();

		var fileUrls = new List<string> { mock.StarskyDotStarskyPath, };

		// Act
		var result = WindowsOpenDesktopApp.OpenApplicationAtUrl(fileUrls, mock.FullFilePath);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task W_OpenApplicationAtUrl_ReturnsTrue_WhenApplicationOpens__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		// Arrange
		var mock = new CreateFakeStarskyUnixBash();
		var fileUrls = new List<string> { mock.StarskyDotStarskyPath, };
		
		await Command.Run("chmod", "+x",
			mock.FullFilePath).Task;
		
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

		var result = WindowsOpenDesktopApp.OpenDefault(["C:\\not-found-74537587345853847345"],
			OSPlatform.Windows);
		Assert.IsFalse(result);
	}
}
