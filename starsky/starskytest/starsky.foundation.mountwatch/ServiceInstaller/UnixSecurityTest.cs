using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller;

[TestClass]
public class UnixSecurityTest
{
	[TestMethod]
	public void IsRunningAsRoot_ReturnsTrue_WhenEuidIsZero()
	{
		var sut = new UnixSecurityZero();
		Assert.IsTrue(sut.IsRunningAsRoot());
	}

	[TestMethod]
	public void IsRunningAsRoot_ReturnsFalse_WhenEuidIsNonZero()
	{
		var sut = new UnixSecurityNonZero();
		Assert.IsFalse(sut.IsRunningAsRoot());
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux)]
	public void IsRunningAsRoot_Linux()
	{
		var sut = new UnixSecurity();
		var result = sut.IsRunningAsRoot();
		Assert.AreEqual(result, IsRoot());
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void IsRunningAsRoot_macOs()
	{
		var sut = new UnixSecurity();
		var result = sut.IsRunningAsRoot();
		Assert.AreEqual(result, IsRootViaWhoAmI());
	}

	private static bool IsRootViaWhoAmI()
	{
		var psi = new ProcessStartInfo
		{
			FileName = "whoami", RedirectStandardOutput = true, UseShellExecute = false
		};

		using var process = Process.Start(psi);
		var output = process!.StandardOutput.ReadToEnd().Trim();
		process.WaitForExit();

		return output == "root";
	}

	private static bool IsRoot()
	{
		if ( !File.Exists("/proc/self/status") )
		{
			return false;
		}

		var lines = File.ReadAllLines("/proc/self/status");
		foreach ( var line in lines )
		{
			if ( !line.StartsWith("Uid:") )
			{
				continue;
			}

			var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
			if ( parts.Length > 1 && int.TryParse(parts[1], out var uid) )
			{
				return uid == 0;
			}
		}

		return false;
	}

	private sealed class UnixSecurityZero : UnixSecurity
	{
		protected override uint GetEuid()
		{
			return 0;
		}
	}

	private sealed class UnixSecurityNonZero : UnixSecurity
	{
		protected override uint GetEuid()
		{
			return 1000;
		}
	}
}
