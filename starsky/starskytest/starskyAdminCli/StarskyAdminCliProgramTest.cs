using System;
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
			Environment.SetEnvironmentVariable("app__databaseType","InMemoryDatabase");
			Environment.SetEnvironmentVariable("app__databaseConnection", "test");
				
			var args = new List<string> {
				"-h","-v",
				"-d", "InMemoryDatabase",
				"-c", "-test"
			}.ToArray();
			Program.Main(args);
			Assert.IsNotNull(args);
		}

	}
}
