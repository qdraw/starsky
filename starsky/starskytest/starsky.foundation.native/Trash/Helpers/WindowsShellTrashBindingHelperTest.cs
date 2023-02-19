using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash;
using starsky.foundation.native.Trash.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.Trash;

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
		Assert.AreEqual( RuntimeInformation.IsOSPlatform(OSPlatform.Windows),result.Item1);

		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			var exists = File.Exists(destPath);
			if ( exists )
			{
				File.Delete(destPath);
			}
			Assert.AreEqual(false, exists);
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
		
		Assert.AreEqual(null, result.Item1);
		Assert.IsTrue(result.Item2.Contains("Not supported"));
	}
	
	[TestMethod]
	public void WindowsShellTrashBindingHelperTest_List_NonWindows()
	{
		var result = WindowsShellTrashBindingHelper.Trash(new List<string>{"destPath"}, OSPlatform.Linux);
		
		Assert.AreEqual(null, result.Item1);
		Assert.IsTrue(result.Item2.Contains("Not supported"));
	}

	[TestMethod]
	public void FileOperationTrash_FOF_SILENT()
	{
		Assert.AreEqual(0x0004, (ushort)WindowsShellTrashBindingHelper.ShFileOperations.FOF_SILENT);
	}

	[TestMethod]
	public void FileOperationTrash_FOF_NOCONFIRMATION()
	{
		Assert.AreEqual(0x0010, (ushort)WindowsShellTrashBindingHelper.ShFileOperations.FOF_NOCONFIRMATION);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_ALLOWUNDO()
	{
		Assert.AreEqual(0x0040, (ushort)WindowsShellTrashBindingHelper.ShFileOperations.FOF_ALLOWUNDO);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_SIMPLEPROGRESS()
	{
		Assert.AreEqual(0x0100, (ushort)WindowsShellTrashBindingHelper.ShFileOperations.FOF_SIMPLEPROGRESS);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_NOERRORUI()
	{
		Assert.AreEqual(0x0400, (ushort)WindowsShellTrashBindingHelper.ShFileOperations.FOF_NOERRORUI);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_WANTNUKEWARNING()
	{
		Assert.AreEqual(0x4000, (ushort)WindowsShellTrashBindingHelper.ShFileOperations.FOF_WANTNUKEWARNING);
	}
	
	[TestMethod]
	public void FileOperationType_FO_MOVE()
	{
		Assert.AreEqual((uint)0x0001, (uint)WindowsShellTrashBindingHelper.FileOperationType.FO_MOVE);
	}
	
	[TestMethod]
	public void FileOperationType_FO_COPY()
	{
		Assert.AreEqual((uint)0x0002, (uint)WindowsShellTrashBindingHelper.FileOperationType.FO_COPY);
	}
	
	[TestMethod]
	public void FileOperationType_FO_DELETE()
	{
		Assert.AreEqual((uint)0x0003, (uint)WindowsShellTrashBindingHelper.FileOperationType.FO_DELETE);
	}
		
	[TestMethod]
	public void FileOperationType_FO_RENAME()
	{
		Assert.AreEqual((uint)0x0004, (uint)WindowsShellTrashBindingHelper.FileOperationType.FO_RENAME);
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
			lpszProgressTitle= "test"
		};
		
		Assert.AreEqual(IntPtr.Zero, t.hwnd);
		Assert.AreEqual(WindowsShellTrashBindingHelper.FileOperationType.FO_COPY, t.wFunc);
		Assert.AreEqual("C:\\test\\test.txt", t.pFrom);
		Assert.AreEqual("C:\\test\\test.txt", t.pTo);
		Assert.AreEqual(WindowsShellTrashBindingHelper.ShFileOperations.FOF_ALLOWUNDO, t.fFlags);
		Assert.AreEqual(true, t.fAnyOperationsAborted);
		Assert.AreEqual(IntPtr.Zero, t.hNameMappings);
		Assert.AreEqual("test", t.lpszProgressTitle);
		
	}

	[TestMethod]
	public void Shqueryrbinfo1()
	{
		var values = new WindowsShellTrashBindingHelper.SHQUERYRBINFO
		{
			cbSize = 20,
			i64Size = 2,
			i64NumItems = 3
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
			Assert.AreEqual(null, hResult);
			Assert.IsTrue(info.Contains("Unable to load shared library"));
			Assert.AreEqual(0, pShQueryRbInfo.i64NumItems);
			return;
		}

		Assert.AreEqual(-2147024893, hResult);
		info ??= WindowsShellTrashBindingHelper.SHQueryRecycleBinInfo(hResult, "ZZ:\\", pShQueryRbInfo);

		Assert.IsTrue(info.Contains("Fail! Drive ZZ:\\ contains 0 item(s) in 0 bytes"));
		Assert.AreEqual(0, pShQueryRbInfo.i64NumItems);
	}

	[TestMethod]
	public void SHQueryRecycleBinInfo1_Success()
	{
		var result = WindowsShellTrashBindingHelper.SHQueryRecycleBinInfo(0, @"C:\",
			new WindowsShellTrashBindingHelper.SHQUERYRBINFO
			{
				i64Size = 10252,
				i64NumItems = 1
			});
		Assert.AreEqual(@"Success! Drive C:\ contains 1 item(s) in " + $"{10252:#,##0} bytes" ,result);
	}
	
	[TestMethod]
	public void SHQueryRecycleBinInfo1_Fail()
	{
		var result = WindowsShellTrashBindingHelper.SHQueryRecycleBinInfo(1, @"C:\",
			new WindowsShellTrashBindingHelper.SHQUERYRBINFO());
		Assert.AreEqual(@"Fail! Drive C:\ contains 0 item(s) in 0 bytes",result);
	}
	
	[TestMethod]
	public void DriveHasRecycleBin_InvalidDrive()
	{
		var (driveHasBin, items, info) = WindowsShellTrashBindingHelper
			.DriveHasRecycleBin("ZZ:\\");
		
		Assert.AreEqual(0,items);
		
		// Shell32.dll is not available on Linux or Mac OS
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			Assert.AreEqual(false, driveHasBin);
			Assert.IsTrue(info.Contains("Unable to load shared library"));
			return;
		}

		Assert.AreEqual(false, driveHasBin);
		Assert.IsTrue(info.Contains("Fail! Drive ZZ:\\ contains 0 item(s) in 0 bytes"));
	}
	
	[TestMethod]
	public void DriveHasRecycleBin_C_Drive()
	{
		var (driveHasBin, items, info) = WindowsShellTrashBindingHelper
			.DriveHasRecycleBin();
		
		// Shell32.dll is not available on Linux or Mac OS
		if ( !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			Assert.AreEqual(0,items);
			Assert.AreEqual(false, driveHasBin);
			Assert.IsTrue(info.Contains("Unable to load shared library"));
			return;
		}

		Assert.AreEqual(true, items >= 0);
		Assert.AreEqual(true, driveHasBin);
	}
}
