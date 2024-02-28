using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers
{
	[TestClass]
	public sealed class Base64HelperTest
	{
		[TestMethod]
		public void Base64HelperTest_ToBase64ToStringEmpty()
		{
			var base64 = Base64Helper.ToBase64(new MemoryStream());
			Assert.AreEqual(string.Empty, base64);
		}

		[TestMethod]
		public void Base64HelperTest_base64String_ToBase64()
		{
			// test == "dGVzdA=="
			var base64String = Base64Helper.EncodeToString("test");
			Assert.AreEqual("dGVzdA==", base64String);
		}

		[TestMethod]
		public void Base64HelperTest_base64bytes_ToBase64()
		{
			// test == "dGVzdA=="
			var base64bytes = Base64Helper.EncodeToBytes("test");
			var base64String = Base64Helper.ToBase64(base64bytes);
			Assert.AreEqual("dGVzdA==", base64String);
		}

		[TestMethod]
		public void Base64HelperTest_TryParseCorruptString()
		{
			var currupt = Base64Helper.TryParse("failing");
			var noByte = Array.Empty<byte>();
			Assert.AreEqual(noByte.Length, currupt.Length);
		}
	}
}
