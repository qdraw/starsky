using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskythumbnailcli;

namespace starskytest.starskythumbnailcli
{
	[TestClass]
	public class ProgramTest
	{
		[TestMethod]
		public void ProgramTest_default()
		{
			var args = new List<string> {"-h", "-v"}.ToArray();
			Program.Main(args);
		}
	}
}
