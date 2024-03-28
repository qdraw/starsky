using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class PlatformParserTest
{
	[TestMethod]
	public void GetCurrentOsPlatformTest()
	{
		var content = PlatformParser.GetCurrentOsPlatform();
		Assert.IsNotNull(content);

		var allOsPlatforms = new List<OSPlatform>
		{
			OSPlatform.Linux, OSPlatform.Windows, OSPlatform.OSX, OSPlatform.FreeBSD
		};
		Assert.IsTrue(allOsPlatforms.Contains(content.Value));
	}

	[DataTestMethod]
	[DataRow("osx-arm64", "OSX")]
	[DataRow("osx-x64", "OSX")]
	[DataRow("linux-x64", "LINUX")]
	[DataRow("linux-arm", "LINUX")]
	[DataRow("linux-arm64", "LINUX")]
	[DataRow("win-x64", "WINDOWS")]
	[DataRow("win-x86", "WINDOWS")]
	[DataRow("win-arm64", "WINDOWS")]
	[DataRow("test", "")]
	[DataRow(null, "")]
	public void RuntimeIdentifierTest(string input, string expected)
	{
		Assert.AreEqual(expected, PlatformParser.RuntimeIdentifier(input).ToString());
	}
}
