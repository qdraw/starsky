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

	private static bool IsWindows(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.Windows;
	}

	private static bool IsMacOs(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.OSX;
	}

	private static bool IsLinux(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.Linux;
	}

	private static bool IsFreeBsd(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.FreeBSD;
	}
	
	private static bool IsUnknown(OSPlatform osPlatform)
	{
		return osPlatform == OSPlatform.Create("Unknown");
	}
	
	[TestMethod]
	public void OperatingSystemHelper_Windows()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(IsWindows);
		Assert.AreEqual(OSPlatform.Windows, result);
	}
	
	[TestMethod]
	public void OperatingSystemHelper_MacOs()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(IsMacOs);
		Assert.AreEqual(OSPlatform.OSX, result);
	}
		
	[TestMethod]
	public void OperatingSystemHelper_Linux()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(IsLinux);
		Assert.AreEqual(OSPlatform.Linux, result);
	}
	
	[TestMethod]
	public void OperatingSystemHelper_FreeBsd()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(IsFreeBsd);
		Assert.AreEqual(OSPlatform.FreeBSD, result);
	}
	
				
	[TestMethod]
	public void OperatingSystemHelper_Other()
	{
		var result = OperatingSystemHelper.GetPlatformInternal(IsUnknown);
		Assert.AreEqual(OSPlatform.Create("Unknown"), result);
	}
}
