using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyimportercli;

namespace starskytest.starskyImporterCli
{
	[TestClass]
	public class starskyImporterCliProgramTest
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
