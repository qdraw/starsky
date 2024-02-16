using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.OpenApplicationNative;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;

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
		catch ( IOException )
		{
			// do nothing
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

		// retry due due multi threading
		if ( result != true )
		{
			Console.WriteLine("retry due due multi threading");
			await Task.Delay(100);
			SetupEnsureAssociationsSet();
			var service = new OpenApplicationNativeService();
			result = service.OpenDefault([mock.StarskyDotStarskyPath]);
		}

		Assert.IsTrue(result);
	}


	[TestMethod]
	public void OpenApplicationAtUrl_ZeroItems_SoFalse()
	{
		var result = new OpenApplicationNativeService().OpenApplicationAtUrl([], "app");

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
	public void OpenApplicationAtUrl_AllApplicationsSupported_ReturnsTrue()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Linux, Mac OS Only");
			return;
		}

		// Arrange
		var service = new OpenApplicationNativeService();
		// List is (File Path and Application URL)

		var fullPathAndApplicationUrl = new List<(string, string)>
		{
			( "file1", new CreateFakeStarskyUnixBash().ApplicationUrl )
		};

		// Act
		var result = service.OpenApplicationAtUrl(fullPathAndApplicationUrl);

		// Assert
		Assert.IsTrue(result);
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
		Assert.IsTrue(result.Any(x => x.Item2 == "app1"));
		Assert.IsTrue(result.Any(x => x.Item2 == "app2"));
		Assert.IsTrue(result.Any(x => x.Item2 == "app3"));
	}
}
