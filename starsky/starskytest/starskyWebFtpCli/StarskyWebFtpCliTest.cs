using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskywebftpcli;

namespace starskytest.starskyWebFtpCli
{
	[TestClass]
	public sealed class StarskyWebFtpCliTest
	{
		[TestMethod]
		public void StarskyCliHelpVerbose()
		{
			var args = new List<string> {
				"-h","-v"
			}.ToArray();
			Program.Main(args);
			// should not crash
			Assert.IsNotNull(args);
		}
	}
}
