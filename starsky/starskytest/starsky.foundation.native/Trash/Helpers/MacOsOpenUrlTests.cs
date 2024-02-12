using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.Trash.Helpers;

[TestClass]
public class MacOsOpenUrlTests
{
	[TestMethod]
	public async Task TestMethodWithSpecificApp()
	{
		var filePath = new CreateAnImage().FullFilePath;

		MacOsOpenUrl.OpenApplicationAtUrl(filePath,
			"/System/Applications/Utilities/Console.app");

		var isProcess = Process.GetProcessesByName("Console").Length > 0;
		for ( var i = 0; i < 10; i++ )
		{
			isProcess = Process.GetProcessesByName("Console").Length > 0;
			if ( isProcess )
			{
				await Command.Run("osascript", "-e", "tell application \"Console\" to if it is running then quit").Task;
				break;
			}

			await Task.Delay(10);
		}
		
		Assert.IsTrue(isProcess);
	}

	[TestMethod]
	public void TestMethodWithDefaultApp()
	{
		var result = MacOsOpenUrl.OpenDefault("urlNotFound");
		Assert.IsFalse(result);
	}
}
