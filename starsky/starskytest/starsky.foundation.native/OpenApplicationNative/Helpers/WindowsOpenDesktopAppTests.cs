using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Medallion.Shell;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class WindowsOpenDesktopAppTests
{
	private const string Extension = ".starsky";
	private const string ProgramId = "starskytest";
	private const string FileTypeDescription = "Starsky Test File";

	[TestInitialize]
	public void TestInitialize()
	{
		SetupEnsureAssociationsSet();
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
		try
		{
			Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{Extension}", false);
			Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{ProgramId}", false);
		}
		catch ( IOException )
		{
			// do nothing
		}
	}

	private static CreateFakeStarskyWindowsExe SetupEnsureAssociationsSet()
	{
		if ( !new AppSettings().IsWindows )
		{
			return new CreateFakeStarskyWindowsExe();
		}

		var mock = new CreateFakeStarskyWindowsExe();
		var filePath = mock.FullFilePath;
		WindowsSetFileAssociations.EnsureAssociationsSet(
			new FileAssociation
			{
				Extension = Extension,
				ProgId = ProgramId,
				FileTypeDescription = FileTypeDescription,
				ExecutableFilePath = filePath
			});
		return mock;
	}


	[TestMethod]
	public void W_OpenDefault_NonWindows()
	{
		var result = WindowsOpenDesktopApp.OpenDefault(["any value"], OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void W_OpenDefault2_NonWindows()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		var result = WindowsOpenDesktopApp.OpenDefault(["any value"], OSPlatform.Windows);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void W_OpenDefault3_NonWindows()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		var result = WindowsOpenDesktopApp.OpenDefault(["any value"]);

		Console.WriteLine(result);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task W_OpenDefault_HappyFlow__WindowsOnly()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var mock = SetupEnsureAssociationsSet();
		var result =
			WindowsOpenDesktopApp.OpenDefault([mock.StarskyDotStarskyPath], OSPlatform.Windows);

		// Retry if failed due to multi-threading
		for ( var i = 0; i < 2 && result != true; i++ )
		{
			Console.WriteLine($"Retry due to multi-threading {i + 1}");
			await Task.Delay(1000);
			SetupEnsureAssociationsSet();
			result = WindowsOpenDesktopApp.OpenDefault([mock.StarskyDotStarskyPath]);
		}

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
	[ExpectedException(typeof(Win32Exception))]
	public void W_OpenApplicationAtUrl2_NonWindows()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		// ExpectedException = Win32Exception
		WindowsOpenDesktopApp.OpenApplicationAtUrl(["any value"],
			"/not_found_849539453", OSPlatform.Windows);
	}

	[TestMethod]
	[ExpectedException(typeof(Win32Exception))]
	public void W_OpenApplicationAtUrl3_NonWindows()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		WindowsOpenDesktopApp.OpenApplicationAtUrl(new List<string> { "any value" },
			"app");
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

	[TestMethod]
	public void WindowsOpenDesktopApp_OpenDefault_Count0()
	{
		var result = WindowsOpenDesktopApp.OpenDefault(new List<string>());
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void WindowsOpenDesktopApp_OpenDefault_Count0_OSLinux()
	{
		var result = WindowsOpenDesktopApp.OpenDefault([], OSPlatform.Linux);
		Assert.IsNull(result);
	}


	[TestMethod]
	public void WindowsOpenDesktopApp_OpenApplicationAtUrl_Count0()
	{
		var result = WindowsOpenDesktopApp.OpenApplicationAtUrl([], string.Empty);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void WindowsOpenDesktopApp_OpenApplicationAtUrl_Count0_OSLinux()
	{
		var result = WindowsOpenDesktopApp.OpenApplicationAtUrl([],
			string.Empty, OSPlatform.Linux);
		Assert.IsNull(result);
	}
}
