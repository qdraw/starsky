using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.Trash.Helpers;

[TestClass]
public class WindowsShellTrashBindingHelperTest
{
	[TestMethod]
	public void WindowsShellTrashBindingHelperTest_ShouldRemove_OnWindows()
	{
		var createAnImage = new CreateAnImage();
		var destPath = Path.Combine(createAnImage.BasePath, "test.jpg");
		File.Copy(createAnImage.FullFilePath, destPath, true);

		var result = WindowsShellTrashBindingHelper.Trash(destPath, OSPlatform.Windows);
		Assert.AreEqual(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), result.Item1);

		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			var exists = File.Exists(destPath);
			if ( exists )
			{
				File.Delete(destPath);
			}

			Assert.IsFalse(exists);
		}

		if ( File.Exists(destPath) )
		{
			File.Delete(destPath);
		}
	}

	[TestMethod]
	public void WindowsShellTrashBindingHelperTest_NonWindows()
	{
		var result = WindowsShellTrashBindingHelper.Trash("destPath", OSPlatform.Linux);

		Assert.IsNull(result.Item1);
		Assert.Contains("Not supported", result.Item2);
	}

	[TestMethod]
	public void WindowsShellTrashBindingHelperTest_List_NonWindows()
	{
		var result =
			WindowsShellTrashBindingHelper.Trash(new List<string> { "destPath" }, OSPlatform.Linux);

		Assert.IsNull(result.Item1);
		Assert.Contains("Not supported", result.Item2);
	}

	[TestMethod]
	[DynamicData(nameof(GetFileOperationTestData))]
	public void FileOperationTests(uint expected, object actual, string name)
	{
		Assert.AreEqual(expected, actual, name);
	}

	public static IEnumerable<object[]> GetFileOperationTestData()
	{
		yield return
		[
			( uint ) 0x0004,
			( uint ) WindowsShellTrashBindingHelper.ShFileOperations.FOF_SILENT,
			"FOF_SILENT"
		];
		yield return
		[
			( uint ) 0x0010,
			( uint ) WindowsShellTrashBindingHelper.ShFileOperations.FOF_NOCONFIRMATION,
			"FOF_NOCONFIRMATION"
		];
		yield return
		[
			( uint ) 0x0040,
			( uint ) WindowsShellTrashBindingHelper.ShFileOperations.FOF_ALLOWUNDO,
			"FOF_ALLOWUNDO"
		];
		yield return
		[
			( uint ) 0x0100,
			( uint ) WindowsShellTrashBindingHelper.ShFileOperations.FOF_SIMPLEPROGRESS,
			"FOF_SIMPLEPROGRESS"
		];
		yield return
		[
			( uint ) 0x0400,
			( uint ) WindowsShellTrashBindingHelper.ShFileOperations.FOF_NOERRORUI,
			"FOF_NOERRORUI"
		];
		yield return
		[
			( uint ) 0x4000,
			( uint ) WindowsShellTrashBindingHelper.ShFileOperations.FOF_WANTNUKEWARNING,
			"FOF_WANTNUKEWARNING"
		];
		yield return
		[
			( uint ) 0x0001,
			( uint ) WindowsShellTrashBindingHelper.FileOperationType.FO_MOVE,
			"FO_MOVE"
		];
		yield return
		[
			( uint ) 0x0002,
			( uint ) WindowsShellTrashBindingHelper.FileOperationType.FO_COPY,
			"FO_COPY"
		];
		yield return
		[
			( uint ) 0x0003,
			( uint ) WindowsShellTrashBindingHelper.FileOperationType.FO_DELETE,
			"FO_DELETE"
		];
		yield return
		[
			( uint ) 0x0004,
			( uint ) WindowsShellTrashBindingHelper.FileOperationType.FO_RENAME,
			"FO_RENAME"
		];
	}

	[TestMethod]
	public void Shfileopstruct1()
	{
		var t = new WindowsShellTrashBindingHelper.SHFILEOPSTRUCT
		{
			hwnd = IntPtr.Zero,
			wFunc =
				WindowsShellTrashBindingHelper.FileOperationType.FO_COPY,
			pFrom = "C:\\test\\test.txt",
			pTo = "C:\\test\\test.txt",
			fFlags = WindowsShellTrashBindingHelper.ShFileOperations.FOF_ALLOWUNDO,
			fAnyOperationsAborted = true,
			hNameMappings = IntPtr.Zero,
			lpszProgressTitle = "test"
		};

		Assert.AreEqual(IntPtr.Zero, t.hwnd);
		Assert.AreEqual(WindowsShellTrashBindingHelper.FileOperationType.FO_COPY, t.wFunc);
		Assert.AreEqual("C:\\test\\test.txt", t.pFrom);
		Assert.AreEqual("C:\\test\\test.txt", t.pTo);
		Assert.AreEqual(WindowsShellTrashBindingHelper.ShFileOperations.FOF_ALLOWUNDO, t.fFlags);
		Assert.IsTrue(t.fAnyOperationsAborted);
		Assert.AreEqual(IntPtr.Zero, t.hNameMappings);
		Assert.AreEqual("test", t.lpszProgressTitle);
	}

	[TestMethod]
	public void Shqueryrbinfo1()
	{
		var values = new WindowsShellTrashBindingHelper.SHQUERYRBINFO
		{
			cbSize = 20, i64Size = 2, i64NumItems = 3
		};

		Assert.AreEqual(20, values.cbSize);
		Assert.AreEqual(2, values.i64Size);
		Assert.AreEqual(3, values.i64NumItems);
	}

	[TestMethod]
	public void SHQueryRecycleBinWrapper_InvalidDrive()
	{
		var (hResult, info, pShQueryRbInfo) = WindowsShellTrashBindingHelper
			.SHQueryRecycleBinWrapper("ZZ:\\");

		// Shell32.dll is not available on Linux or Mac OS
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			Assert.IsNull(hResult);
			Assert.IsTrue(info?.Contains("Unable to load shared library"));
			Assert.AreEqual(0, pShQueryRbInfo.i64NumItems);
			return;
		}

		Assert.IsTrue(hResult is -2147024893 or -2147024894);

		info ??= WindowsShellTrashBindingHelper.SHQueryRecycleBinInfo(hResult, "ZZ:\\",
			pShQueryRbInfo);

		Assert.Contains("Fail! Drive ZZ:\\ contains 0 item(s) in 0 bytes", info);
		Assert.AreEqual(0, pShQueryRbInfo.i64NumItems);
	}

	[TestMethod]
	public void SHQueryRecycleBinInfo1_Success()
	{
		var result = WindowsShellTrashBindingHelper.SHQueryRecycleBinInfo(0, @"C:\",
			new WindowsShellTrashBindingHelper.SHQUERYRBINFO { i64Size = 10252, i64NumItems = 1 });
		Assert.AreEqual(@"Success! Drive C:\ contains 1 item(s) in " + $"{10252:#,##0} bytes",
			result);
	}

	[TestMethod]
	public void SHQueryRecycleBinInfo1_Fail()
	{
		var result = WindowsShellTrashBindingHelper.SHQueryRecycleBinInfo(1, @"C:\",
			new WindowsShellTrashBindingHelper.SHQUERYRBINFO());
		Assert.AreEqual(@"Fail! Drive C:\ contains 0 item(s) in 0 bytes", result);
	}

	[TestMethod]
	public void DriveHasRecycleBin_InvalidDrive()
	{
		var (driveHasBin, items, info) = WindowsShellTrashBindingHelper
			.DriveHasRecycleBin("ZZ:\\");

		Assert.AreEqual(0, items);

		// Shell32.dll is not available on Linux or Mac OS
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			Assert.IsFalse(driveHasBin);
			Assert.Contains("Unable to load shared library", info);
			return;
		}

		Assert.IsFalse(driveHasBin);
		Assert.Contains("Fail! Drive ZZ:\\ contains 0 item(s) in 0 bytes", info);
	}

	[TestMethod]
	public void DriveHasRecycleBin_C_Drive()
	{
		var (driveHasBin, items, info) = WindowsShellTrashBindingHelper
			.DriveHasRecycleBin();

		// Shell32.dll is not available on Linux or Mac OS
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			Assert.AreEqual(0, items);
			Assert.IsFalse(driveHasBin);
			Assert.Contains("Unable to load shared library", info);
			Assert.Inconclusive("Shell32.dll is not available on Linux or Mac OS");
			return;
		}

		Assert.IsGreaterThanOrEqualTo(0, items);
		Assert.IsTrue(driveHasBin);
	}
}
