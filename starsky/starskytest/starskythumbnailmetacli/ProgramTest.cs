using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskythumbnailmetacli;

namespace starskytest.starskythumbnailmetacli
{
	[TestClass]
	public class ProgramTest
	{
		[TestMethod]
		public async Task ProgramTest_default()
		{
			var args = new List<string> {"-h", "-v"}.ToArray();
			await Program.Main(args);
			Assert.IsNotNull(args);
		}
	}
}
