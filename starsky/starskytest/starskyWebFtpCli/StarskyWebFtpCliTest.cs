using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskywebftpcli;

namespace starskytest.starskyWebFtpCli
{
	[TestClass]
	public sealed class StarskyWebFtpCliTest
	{
		[TestMethod]
		public async Task StarskyCliHelpVerbose()
		{
			var args = new List<string> { "-h", "-v" }.ToArray();

			await Program.Main(args);
			// should not crash
			Assert.IsNotNull(args);
		}
	}
}
