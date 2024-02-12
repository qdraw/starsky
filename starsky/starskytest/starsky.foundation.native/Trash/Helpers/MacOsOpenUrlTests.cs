using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.Trash.Helpers;

[TestClass]
public class MacOsOpenUrlTests
{
	[TestMethod]
	public void TestMethodWithSpecificApp()
	{
		var filePath = new CreateAnImage().FullFilePath;
		
		MacOsOpenUrl.OpenApplicationAtUrl(filePath,
			"/System/Applications/Preview.app");
		
		Thread.Sleep(1000);
		Console.WriteLine();
	}
	
	[TestMethod]
	public void TestMethodWithDefaultApp()
	{
		var filePath = new CreateAnImage().FullFilePath;
		
		MacOsOpenUrl.OpenDefault(filePath);
		
		Thread.Sleep(1000);
	}
}
