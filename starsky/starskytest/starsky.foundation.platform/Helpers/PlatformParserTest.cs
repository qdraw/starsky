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
	[DataRow("osx-arm64", "OSX", "Arm64")]
	[DataRow("osx-x64", "OSX", "X64")]
	[DataRow("linux-x64", "LINUX", "X64")]
	[DataRow("linux-arm", "LINUX", "Arm")]
	[DataRow("linux-arm64", "LINUX", "Arm64")]
	[DataRow("win-x64", "WINDOWS", "X64")]
	[DataRow("win-x86", "WINDOWS", "X86")]
	[DataRow("win-arm64", "WINDOWS", "Arm64")]
	[DataRow("test", "", "")]
	[DataRow(null, "", "")]
	public void RuntimeIdentifierTest(string input, string expectedOs, string expectedArch)
	{
		var result = PlatformParser.RuntimeIdentifier(input);
		Assert.AreEqual(expectedOs, result[0].Item1.ToString());
		Assert.AreEqual(expectedArch, result[0].Item2.ToString());
	}
}
