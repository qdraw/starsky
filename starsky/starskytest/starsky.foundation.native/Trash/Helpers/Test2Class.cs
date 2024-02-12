using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash.Helpers;

namespace starskytest.starsky.foundation.native.Trash.Helpers;

[TestClass]
public class Test2Class
{
	[TestMethod]
	public void TestMethod1()
	{
		MacOsOpenUrl.OpenApplicationAtUrl("/Users/dion/Desktop/rosseta.png",
			"/Applications/Adobe Photoshop 2024/Adobe Photoshop 2024.app");
		
		// MacOsTest2.OpenDefault("/Users/dion/Desktop/rosseta.png");
		Console.WriteLine();
	}
}
