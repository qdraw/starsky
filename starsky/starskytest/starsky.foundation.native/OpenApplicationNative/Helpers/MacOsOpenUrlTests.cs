using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Architecture;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class MacOsOpenUrlTests
{
	private const string ConsoleApp = "/System/Applications/Utilities/Console.app";
	private const string ConsoleName = "Console";

	[TestMethod]
	public void OpenDefault_NonMacOS()
	{
		var result = MacOsOpenUrl.OpenDefault(["OpenDefault_NonMacOS any value"], OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void OpenDefault__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for Windows / Linux only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsOpenUrl.OpenDefault(["not important"], OSPlatform.OSX));
	}

	[TestMethod]
	public async Task TestMethodWithSpecificApp__MacOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Mac OS Only");
			return;
		}

		var filePath = new CreateAnImage().FullFilePath;

		MacOsOpenUrl.OpenApplicationAtUrl([filePath], ConsoleApp);

		var isProcess = Process.GetProcessesByName(ConsoleName).ToList()
			.Exists(p => p.MainModule?.FileName.Contains(ConsoleApp) == true);

		await Task.Delay(10);

		for ( var i = 0; i < 60; i++ )
		{
			isProcess = Process.GetProcessesByName(ConsoleName).ToList()
				.Exists(p => p.MainModule?.FileName.Contains(ConsoleApp) == true);

			if ( isProcess )
			{
				await Command.Run("osascript", "-e",
					"tell application \"Console\" to if it is running then quit").Task;
				break;
			}

			await Task.Delay(5);
		}

		Assert.IsTrue(isProcess);
	}

	[TestMethod]
	public void OpenApplicationAtUrl_NoItems()
	{
		var result = MacOsOpenUrl.OpenApplicationAtUrl([], ConsoleApp);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void OpenDefault_NoItems()
	{
		var result = MacOsOpenUrl.OpenDefault([]);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TestMethodWithDefaultApp__MacOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Mac OS Only");
			return;
		}

		var result = MacOsOpenUrl.OpenDefault(["urlNotFound"]);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void OpenApplicationAtUrl_NonMacOs()
	{
		var result = MacOsOpenUrl.OpenApplicationAtUrl(
			new List<string> { "OpenApplicationAtUrl_NonMacOs any value" }, "app",
			OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void OpenApplicationAtUrl__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for Windows / Linux only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsOpenUrl.OpenApplicationAtUrl(["not important"], "not important", OSPlatform.OSX));
	}

	[TestMethod]
	public void OpenURLsWithApplicationAtURL__NonMacOS1()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for Windows / Linux only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsOpenUrl.OpenUrLsWithApplicationAtUrl(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
	}

	[TestMethod]
	public void NsWorkspaceSharedWorkSpace__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for Windows / Linux only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsOpenUrl.NsWorkspaceSharedWorkSpace());
	}

	[TestMethod]
	public void InvokeOpenUrl__NonMacOS()
	{
		// Arrange
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for Windows/Linux only.");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() => MacOsOpenUrl.InvokeOpenUrl(IntPtr.Zero));
	}
}
