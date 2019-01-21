using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starskycore.Attributes;
using starskycore.Services;

namespace starskytests.Services
{
	[TestClass]
	public class Base32Test
	{
		[TestMethod]
		public void Base32EncodeDecodeTest()
		{
			var encodeBytes = Base32.Decode("VWQII6FKPI26LLWB3TRDQJU3CM");
			var encodeString = Base32.Encode(encodeBytes);
			Assert.AreEqual("VWQII6FKPI26LLWB3TRDQJU3CM",encodeString);
			
			// Empty result
			encodeString = Base32.Encode(new byte[0]);
			Assert.AreEqual(string.Empty,encodeString);
			
			Base32.Encode(encodeBytes,true);
		}
	
	}
}