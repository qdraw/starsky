using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Services;

namespace starskytest.Services
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

		[TestMethod]
		[ExpectedException(typeof(Base32.DecodingException))]
		public void Base32EncodeDecodeTestFail()
		{
			Base32.Decode("54678945346"); // will fail
		}

		[TestMethod]
		public void Base32DecodeNull()
		{
			var encodeBytes = Base32.Decode(string.Empty); 
			Assert.AreEqual(0,encodeBytes.Length);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Base32Encode_OutRange()
		{
			 // the length (1 << 28 )
			Base32.Encode(new byte[268435456]);
		}

	}
}
