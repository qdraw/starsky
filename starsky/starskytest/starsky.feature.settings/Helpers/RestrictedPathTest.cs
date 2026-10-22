using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.settings.Helpers;

namespace starskytest.starsky.feature.settings.Helpers;

[TestClass]
public class RestrictedPathTest
{
	[TestMethod]
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public void IsRestrictedPath_SafeDataPath_ReturnsFalse()
	{
		Assert.IsFalse(RestrictedPath.IsRestrictedPath("/data/photos"));
		Assert.IsFalse(RestrictedPath.IsRestrictedPath("/mnt/archive/2024"));
		Assert.IsFalse(RestrictedPath.IsRestrictedPath("/home/user/pictures"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public void IsRestrictedPath_SystemPaths_ReturnsTrue__UnixOnly()
	{
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/etc"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/etc/ssh"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/root"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/boot"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/sys"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/proc"));
		// Null character in path. (Parameter 'path')
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("\0"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public void IsRestrictedPath_OptVarTmp_IncludingSubFolders_ReturnsTrue__UnixOnly()
	{
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/opt"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/opt/starsky"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/var"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/var/lib/data"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/tmp"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/tmp/photos"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public void IsRestrictedPath_UserDataRoots_OnlyTheRootItself_ReturnsTrue__UnixOnly()
	{
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/home"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/home/"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/Users"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/mnt"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/mnt/../mnt"));
		// sub folders stay allowed, these are the normal places for photos
		Assert.IsFalse(RestrictedPath.IsRestrictedPath("/Users/someone/Pictures"));
		Assert.IsFalse(RestrictedPath.IsRestrictedPath("/mnt/archive"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public void IsRestrictedPath_MacOsPaths_ReturnsTrue__MacOnly()
	{
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/System"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/Library"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/private/etc"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath("/private/var"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Windows)]
	public void IsRestrictedPath_WindowsPaths_ReturnsTrue__WindowsOnly()
	{
		Assert.IsTrue(RestrictedPath.IsRestrictedPath(@"C:\Windows"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath(@"C:\Windows\System32"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath(@"C:\Program Files"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath(@"C:\Program Files (x86)"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath(@"C:\ProgramData"));
		Assert.IsTrue(RestrictedPath.IsRestrictedPath(@"C:\System Volume Information"));
	}
}
