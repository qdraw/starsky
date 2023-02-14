using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Trash;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Trash;

[TestClass]
public class WindowsShellTrashBindingHelperTest
{
	
	[TestMethod]
	public void WindowsShellTrashBindingHelper1()
	{
		var service = new WindowsShellTrashBindingHelper(new FakeIWebLogger());
		var result = service.Send("C:\\temp\\test.bmp");
		Assert.AreEqual(false,result.Item1);
	}
}
