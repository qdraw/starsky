using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;

namespace starskytest.starsky.foundation.import.Helpers;

[TestClass]
public class MacOsFileSystemHelperTest
{
	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void GetFileSystem_OnRoot_ReturnsNonEmpty_OnMacOnly()
	{
		var fs = MacOsFileSystemHelper.GetFileSystem("/");
		Assert.IsFalse(string.IsNullOrWhiteSpace(fs), "Filesystem for / should not be empty");
		Console.WriteLine($"Root filesystem: {fs}");
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux)]
	public void GetFileSystem_OnLinux_ThrowsOrFails()
	{
		// On Linux we expect the macOS-specific statfs wrapper to either throw (interop mismatch)
		// or return an empty/invalid string. Accept either outcome.
		bool success;
		try
		{
			var fs = MacOsFileSystemHelper.GetFileSystem("/");
			success = string.IsNullOrWhiteSpace(fs);
		}
		catch
		{
			success = true;
		}

		Assert.IsTrue(success,
			"GetFileSystem did not throw and returned a non-empty value on Linux");
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Windows)]
	public void GetFileSystem_OnWindows_Throws()
	{
		var success = false;
		try
		{
			var fs = MacOsFileSystemHelper.GetFileSystem("/");
			if ( string.IsNullOrWhiteSpace(fs) )
			{
				success = true;
			}
		}
		catch ( DllNotFoundException )
		{
			success = true;
		}
		catch
		{
			// Any other exception is also acceptable here
			success = true;
		}

		Assert.IsTrue(success, "GetFileSystem did not throw or return empty on Windows");
	}
}
