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
}
