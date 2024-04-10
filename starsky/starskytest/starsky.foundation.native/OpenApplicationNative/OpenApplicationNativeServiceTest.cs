using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.OpenApplicationNative;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;
using starskytest.starsky.foundation.native.Helpers;

namespace starskytest.starsky.foundation.native.OpenApplicationNative;

[TestClass]
public class OpenApplicationNativeServiceTest
{
	private const string Extension = ".starsky";
	private const string ProgramId = "starskytest";
	private const string FileTypeDescription = "Starsky Test File";

	[TestInitialize]
	public void TestInitialize()
	{
		SetupEnsureAssociationsSet();
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
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{Extension}", false);
			Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgramId}", false);
		}
		catch ( IOException exception)
		{
			Console.WriteLine($"[CleanSetup] Skip due IOException {exception.Message}");
		}
		catch ( UnauthorizedAccessException exception)
		{
			Console.WriteLine($"[CleanSetup] Skip due UnauthorizedAccessException {exception.Message}");
		}
			
	}

	[TestMethod]
	public async Task Service_OpenDefault_HappyFlow__WindowsOnly()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var mock = SetupEnsureAssociationsSet();

		var result =
			new OpenApplicationNativeService().OpenDefault([mock.StarskyDotStarskyPath]);

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
	public void OpenApplicationAtUrl_ZeroItems_SoFalse()
	{
		var result = OpenApplicationNativeService.OpenApplicationAtUrl([], "app");

		// Linux and FreeBSD are not supported
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.Linux ||
		     OperatingSystemHelper.GetPlatform() == OSPlatform.FreeBSD )
		{
			Assert.IsNull(result);
			return;
		}

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void OpenDefault_ZeroItemsSo_False()
	{
		var result = new OpenApplicationNativeService().OpenDefault([]);

		// Linux and FreeBSD are not supported
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.Linux ||
		     OperatingSystemHelper.GetPlatform() == OSPlatform.FreeBSD )
		{
			Assert.IsNull(result);
			return;
		}

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void OpenApplicationAtUrl_AllApplicationsSupported_ReturnsTrue__LinuxOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.Linux )
		{
			Assert.Inconclusive("This test if for Linux Only");
			return;
		}

		// Arrange
		var service = new OpenApplicationNativeService();
		// List is (File Path and Application URL)

		var fullPathAndApplicationUrl = new List<(string, string)>
		{
			( new CreateFakeStarskyUnixBash().StarskyDotStarskyPath,
				new CreateFakeStarskyUnixBash().ApplicationUrl )
		};

		// Act
		var result = service.OpenApplicationAtUrl(fullPathAndApplicationUrl);

		// Assert
		Assert.IsNull(result);
	}


	[TestMethod]
	public void OpenApplicationAtUrl_NoApplications_ReturnsFalse()
	{
		// Arrange
		var service = new OpenApplicationNativeService();
		var fullPathAndApplicationUrl = new List<(string, string)>();

		// Act
		var result = service.OpenApplicationAtUrl(fullPathAndApplicationUrl);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void SortToOpenFilesByApplicationPath_EmptyList_ReturnsEmptyList()
	{
		// Arrange
		var fullPathAndApplicationUrl = new List<(string, string)>();

		// Act
		var result =
			OpenApplicationNativeService
				.SortToOpenFilesByApplicationPath(fullPathAndApplicationUrl);

		// Assert
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void SortToOpenFilesByApplicationPath_SingleApplication_ReturnsSingleGroup()
	{
		// Arrange
		var fullPathAndApplicationUrl = new List<(string, string)>
		{
			( "file1", "app1" ), ( "file2", "app1" ), ( "file3", "app1" )
		};

		// Act
		var result =
			OpenApplicationNativeService
				.SortToOpenFilesByApplicationPath(fullPathAndApplicationUrl);

		// Assert
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(3, result[0].Item1.Count);
		Assert.AreEqual("app1", result[0].Item2);
	}

	[TestMethod]
	public void SortToOpenFilesByApplicationPath_MultipleApplications_ReturnsMultipleGroups()
	{
		// Arrange
		var fullPathAndApplicationUrl = new List<(string, string)>
		{
			( "file1", "app1" ),
			( "file2", "app2" ),
			( "file3", "app1" ),
			( "file4", "app2" ),
			( "file5", "app3" )
		};

		// Act
		var result =
			OpenApplicationNativeService
				.SortToOpenFilesByApplicationPath(fullPathAndApplicationUrl);

		// Assert
		Assert.AreEqual(3, result.Count);
		Assert.IsTrue(result.Exists(x => x.Item2 == "app1"));
		Assert.IsTrue(result.Exists(x => x.Item2 == "app2"));
		Assert.IsTrue(result.Exists(x => x.Item2 == "app3"));
	}

	[TestMethod]
	public void DetectToUseOpenApplication_Default()
	{
		var result = new OpenApplicationNativeService().DetectToUseOpenApplication();

		// Depending on the environment
		if ( !Environment.UserInteractive && new AppSettings().IsWindows )
		{
			Assert.IsFalse(result);
			return;
		}

		// Linux and FreeBSD are not supported
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.Linux ||
		     OperatingSystemHelper.GetPlatform() == OSPlatform.FreeBSD )
		{
			Assert.IsFalse(result);
			return;
		}

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DetectToUseOpenApplicationInternal_Windows_AsWindowsService_InteractiveFalse()
	{
		var result =
			OpenApplicationNativeService.DetectToUseOpenApplicationInternal(
				FakeOsOverwrite.IsWindows,
				false);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void DetectToUseOpenApplicationInternal_MacOS_AsLaunchService_InteractiveTrue()
	{
		var result =
			OpenApplicationNativeService.DetectToUseOpenApplicationInternal(FakeOsOverwrite.IsMacOs,
				false);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DetectToUseOpenApplicationInternal_MacOS_Interactive_InteractiveTrue()
	{
		var result =
			OpenApplicationNativeService.DetectToUseOpenApplicationInternal(FakeOsOverwrite.IsMacOs,
				true);
		Assert.IsTrue(result);
	}


	[TestMethod]
	public void DetectToUseOpenApplicationInternal_Linux_Interactive_Interactive_False()
	{
		var result =
			OpenApplicationNativeService.DetectToUseOpenApplicationInternal(FakeOsOverwrite.IsLinux,
				true);
		Assert.IsFalse(result);
	}
}
