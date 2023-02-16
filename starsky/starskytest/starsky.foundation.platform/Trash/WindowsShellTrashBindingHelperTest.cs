using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.platform.Trash;

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
	public void FileOperationTrash_FOF_SILENT()
	{
		Assert.AreEqual(0x0004, (ushort)WindowsShellTrashBindingHelper.FileOperationTrash.FOF_SILENT);
	}

	[TestMethod]
	public void FileOperationTrash_FOF_NOCONFIRMATION()
	{
		Assert.AreEqual(0x0010, (ushort)WindowsShellTrashBindingHelper.FileOperationTrash.FOF_NOCONFIRMATION);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_ALLOWUNDO()
	{
		Assert.AreEqual(0x0040, (ushort)WindowsShellTrashBindingHelper.FileOperationTrash.FOF_ALLOWUNDO);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_SIMPLEPROGRESS()
	{
		Assert.AreEqual(0x0100, (ushort)WindowsShellTrashBindingHelper.FileOperationTrash.FOF_SIMPLEPROGRESS);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_NOERRORUI()
	{
		Assert.AreEqual(0x0400, (ushort)WindowsShellTrashBindingHelper.FileOperationTrash.FOF_NOERRORUI);
	}
	
	[TestMethod]
	public void FileOperationTrash_FOF_WANTNUKEWARNING()
	{
		Assert.AreEqual(0x4000, (ushort)WindowsShellTrashBindingHelper.FileOperationTrash.FOF_WANTNUKEWARNING);
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
}
