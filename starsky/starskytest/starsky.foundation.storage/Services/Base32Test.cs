﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Exceptions;
using starsky.foundation.storage.Services;

namespace starskytest.starsky.foundation.storage.Services
{
	[TestClass]
	public sealed class Base32Test
	{
		[TestMethod]
		public void Base32EncodeDecodeTest()
		{
			var encodeBytes = Base32.Decode("VWQII6FKPI26LLWB3TRDQJU3CM");
			var encodeString = Base32.Encode(encodeBytes);
			Assert.AreEqual("VWQII6FKPI26LLWB3TRDQJU3CM",encodeString);
			
			// Empty result
			encodeString = Base32.Encode(Array.Empty<byte>());
			Assert.AreEqual(string.Empty,encodeString);
			
			Base32.Encode(encodeBytes,true);
		}

		[TestMethod]
		[ExpectedException(typeof(DecodingException))]
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
