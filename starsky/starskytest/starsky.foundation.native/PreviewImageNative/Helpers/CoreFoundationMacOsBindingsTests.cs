using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative.Helpers;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers;

[TestClass]
public class CoreFoundationMacOsBindingsTests
{
	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.OSX)]
	public void CreateCFStringCreateWithCString_Returns0__WindowsLinuxOnly()
	{
		// Act
		var result = CoreFoundationMacOsBindings.CreateCFStringCreateWithCString("input.jpg");
		Assert.AreEqual(0, result);
	}

	[TestMethod]
	public void Size()
	{
		var result = new QuicklookMacOs.CGSize(10, 14);
		Assert.AreEqual(10, result.Width);
		Assert.AreEqual(14, result.Height);
	}

	[TestMethod]
	[DataRow(CoreFoundationMacOsBindings.CFURLPathStyle.HFS)]
	[DataRow(CoreFoundationMacOsBindings.CFURLPathStyle.POSIX)]
	[DataRow(CoreFoundationMacOsBindings.CFURLPathStyle.Windows)]
	public void CfurlPathStyle(CoreFoundationMacOsBindings.CFURLPathStyle type)
	{
		switch ( type )
		{
			case CoreFoundationMacOsBindings.CFURLPathStyle.HFS:
				Assert.AreEqual(1, ( int ) type);
				break;
			case CoreFoundationMacOsBindings.CFURLPathStyle.POSIX:
				Assert.AreEqual(0, ( int ) type);
				break;
			case CoreFoundationMacOsBindings.CFURLPathStyle.Windows:
				Assert.AreEqual(2, ( int ) type);
				break;
		}
	}
}
