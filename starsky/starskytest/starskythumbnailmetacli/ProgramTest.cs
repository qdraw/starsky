using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskythumbnailmetacli;

namespace starskytest.starskythumbnailmetacli
{
	[TestClass]
	public sealed class ProgramTest
	{
		[TestMethod]
		public async Task ProgramTest_default()
		{
			Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup","true");
			Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup","true");
			Environment.SetEnvironmentVariable("app__EnablePackageTelemetry","false");
			
			var args = new List<string> {"-h", "-v"}.ToArray();
			await Program.Main(args);
			Assert.IsNotNull(args);
		}
	}
}
