using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Helpers;

namespace starskytest.starsky.foundation.native.Helpers;

[TestClass]
public class OperatingSystemHelperTest
{
	[TestMethod]
	public void OperatingSystemHelper1()
	{
		var result = OperatingSystemHelper.GetPlatform();
		Assert.IsNotNull(result);
	}


	
	[TestMethod]
	public void OperatingSystemHelper_Windows()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(FakeOsOverwrite.IsWindows);
		Assert.AreEqual(OSPlatform.Windows, result);
	}
	
	[TestMethod]
	public void OperatingSystemHelper_MacOs()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(FakeOsOverwrite.IsMacOs);
		Assert.AreEqual(OSPlatform.OSX, result);
	}
		
	[TestMethod]
	public void OperatingSystemHelper_Linux()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(FakeOsOverwrite.IsLinux);
		Assert.AreEqual(OSPlatform.Linux, result);
	}
	
	[TestMethod]
	public void OperatingSystemHelper_FreeBsd()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(FakeOsOverwrite.IsFreeBsd);
		Assert.AreEqual(OSPlatform.FreeBSD, result);
	}
	
				
	[TestMethod]
	public void OperatingSystemHelper_Other()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(FakeOsOverwrite.IsUnknown);
		Assert.AreEqual(OSPlatform.Create("Unknown"), result);
	}
}
