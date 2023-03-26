using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskydemoseedcli;

namespace starskytest.starskyDemoCli
{
	[TestClass]
	public sealed class StarskyDemoCliProgramTest
	{
		[TestMethod]
		public async Task StarskyDemoCliProgramTest_Help()
		{
			Environment.SetEnvironmentVariable("app__databaseType",
				"InMemoryDatabase");
			Environment.SetEnvironmentVariable("app__databaseConnection",
				"test");
			Environment.SetEnvironmentVariable("app__EnablePackageTelemetry",
				"false");

			var args = new List<string>
			{
				"-h",
				"-v",
				"-d",
				"InMemoryDatabase",
				"-c",
				"-test"
			}.ToArray();
			await Program.Main(args);
			// should not throw an exception
			Assert.IsNotNull(args);
		}
	}
}
