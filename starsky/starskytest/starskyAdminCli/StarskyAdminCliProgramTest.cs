using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyAdminCli;

namespace starskytest.starskyAdminCli
{
	[TestClass]
	public sealed class StarskyAdminCliProgramTest
	{
		[TestMethod]
		public async Task StarskyAdminCliProgramTest_Help()
		{
			Environment.SetEnvironmentVariable("app__databaseType","InMemoryDatabase");
			Environment.SetEnvironmentVariable("app__databaseConnection", "test");
			Environment.SetEnvironmentVariable("app__EnablePackageTelemetry","false");
				
			var args = new List<string> {
				"-h","-v",
				"-d", "InMemoryDatabase",
				"-c", "-test"
			}.ToArray();
			await Program.Main(args);
			// should not throw an exception
			Assert.IsNotNull(args);
		}
		
		[TestMethod]
		public async Task StarskyAdminCliProgramTest_LoopThough()
		{
			Environment.SetEnvironmentVariable("app__databaseType","InMemoryDatabase");
			Environment.SetEnvironmentVariable("app__databaseConnection", "test");
				
			var args = new List<string> {
				"-d", "InMemoryDatabase",
				"-c", "-test",
				"--name", "test@mail.me",
				"--password", "test123456789",
			}.ToArray();
			await Program.Main(args);
			// should not throw an exception
			Assert.IsNotNull(args);
		}

	}
}
