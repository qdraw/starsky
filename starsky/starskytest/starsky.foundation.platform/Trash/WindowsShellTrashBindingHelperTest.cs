using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Trash;

namespace starskytest.starsky.foundation.platform.Trash;

[TestClass]
public class WindowsShellTrashBindingHelperTest
{
	[TestMethod]
	public void Shfileopstruct1()
	{
		var result = new WindowsShellTrashBindingHelper.SHFILEOPSTRUCT();
		result.wFunc = WindowsShellTrashBindingHelper.FileOperationType.FO_DELETE;
		result.pFrom = "test" + '\0' + '\0';
		result.fFlags = WindowsShellTrashBindingHelper.FileOperationFlags.FOF_NOCONFIRMATION;
		Assert.AreEqual(WindowsShellTrashBindingHelper.FileOperationType.FO_DELETE,result.wFunc);
	}
	
	[TestMethod]
	public void WindowsShellTrashBindingHelper1()
	{
		var result = WindowsShellTrashBindingHelper.DeleteFileOperation("test");
		Assert.AreEqual(null,result);
	}
}
