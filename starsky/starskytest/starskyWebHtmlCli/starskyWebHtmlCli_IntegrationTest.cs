using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskywebhtmlcli;

namespace starskytest.starskyWebHtmlCli
{
	[TestClass]
	public sealed class starskyWebHtmlCli_IntegrationTest
	{
		[TestMethod]
		public async Task starskyWebHtmlCli_IntegrationTest_NotFoundTest()
		{
			var args = new List<string> {
				"-p", "not-found-folder" ,"-n", "testrun", "-d", "InMemoryDatabase"
			}.ToArray();
			await Program.Main(args);
			// see console log ==> Please add a valid folder: not-found-folder
			Assert.IsNotNull(args);
		}
        
		[TestMethod]
		public async Task starskyWebHtmlCli_IntegrationTest_NoPath()
		{
			var args = new List<string> {"-d", "InMemoryDatabase"}.ToArray();
			await Program.Main(args);
			// There is a console log
			Assert.IsNotNull(args);
		}
	}
}
