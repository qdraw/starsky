using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class Sha256Test
{
	[TestMethod]
	public void Sha256_1()
	{
		var result = Sha256.ComputeSha256("test");
		Assert.AreEqual("9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08",result);
	}
	
	[TestMethod]
	public void Sha256_Null()
	{
		var result = Sha256.ComputeSha256(null);
		Assert.AreEqual(string.Empty,result);
	}
}
