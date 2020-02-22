using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyAdminCli;

namespace starskytest.starskyAdminCli
{
	[TestClass]
	public class StarskyAdminCliProgramTest
	{
		[TestMethod]
		public void StarskyAdminCliProgramTest_Help()
		{
			var args = new List<string> {
				"-h","-v",
				"-d", "InMemoryDatabase",
			}.ToArray();
			Program.Main(args);
		}
	}
}
