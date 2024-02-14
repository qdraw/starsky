using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class MacOsOpenUrlTests
{
	[TestMethod]
	public void OpenDefault_NonMacOS()
	{
		var result = MacOsOpenUrl.OpenDefault(["any value"], OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	[ExpectedException(typeof(DllNotFoundException))]
	public void OpenDefault__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Windows / Linux only");
			return;
		}

		MacOsOpenUrl.OpenDefault(["not important"], OSPlatform.OSX);
	}


	private const string ConsoleApp = "/System/Applications/Utilities/Console.app";
	private const string ConsoleName = "Console";

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
		for ( var i = 0; i < 15; i++ )
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
		var result = MacOsOpenUrl.OpenApplicationAtUrl(new List<string> { "any value" }, "app",
			OSPlatform.Linux);
		Assert.IsNull(result);
	}

	[TestMethod]
	[ExpectedException(typeof(DllNotFoundException))]
	public void OpenApplicationAtUrl__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Windows / Linux only");
			return;
		}

		MacOsOpenUrl.OpenApplicationAtUrl(["not important"], "not important", OSPlatform.OSX);
	}

	[TestMethod]
	[ExpectedException(typeof(DllNotFoundException))]
	public void OpenURLsWithApplicationAtURL__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Windows / Linux only");
			return;
		}

		MacOsOpenUrl.OpenUrLsWithApplicationAtUrl(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
	}

	[TestMethod]
	[ExpectedException(typeof(DllNotFoundException))]
	public void NsWorkspaceSharedWorkSpace__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Windows / Linux only");
			return;
		}

		MacOsOpenUrl.NsWorkspaceSharedWorkSpace();
	}

	[TestMethod]
	[ExpectedException(typeof(DllNotFoundException))]
	public void InvokeOpenUrl__NonMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Windows / Linux only");
			return;
		}

		MacOsOpenUrl.InvokeOpenUrl(IntPtr.Zero);
	}
}
