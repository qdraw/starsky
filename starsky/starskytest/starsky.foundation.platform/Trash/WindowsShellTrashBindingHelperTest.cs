using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash;

namespace starskytest.starsky.foundation.platform.Trash;

[TestClass]
public class WindowsShellTrashBindingHelperTest
{
	
	[TestMethod]
	public void WindowsShellTrashBindingHelper1()
	{
		var result = WindowsShellTrashBindingHelper.Trash("C:\\temp\\test.bmp", OSPlatform.Windows);
		Assert.AreEqual(false,result.Item1);
	}
	
	[TestMethod]
	public void WindowsShellTrashBindingHelper12()
	{
		var result = WindowsShellTrashBindingHelper.Trash("C:\\temp\\test.bmp", OSPlatform.Windows);
		Assert.AreEqual(false,result.Item1);
	}
}
