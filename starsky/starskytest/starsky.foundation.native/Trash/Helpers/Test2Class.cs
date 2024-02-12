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
		MacOsTest2.OpenApplicationAtURL("/Applications/TextEdit.app");
		Console.WriteLine();
		
	}
}
