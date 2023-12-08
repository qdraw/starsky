using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.storage.Storage
{
	[TestClass]
	public sealed class RetryStreamTest
	{
		[TestMethod]
		public void ReturnedThe3TimeAnStream()
		{
			var i = 0;
			Stream LocalGet()
			{
				i++;
				if ( i != 3 ) throw new IOException();
				return new MemoryStream(CreateAnImageNoExif.Bytes.ToArray());
			}

			var stream = new RetryStream(0).Retry(LocalGet);
			Assert.IsTrue(stream.Length != 0);
		}
		
		[TestMethod]
		[Timeout(3000)]
		public void EndlessFail()
		{
			Stream LocalGet()
			{
				throw new IOException();
			}

			var stream = new RetryStream(0).Retry(LocalGet);
			Assert.AreEqual(0, stream.Length);
		}
	}
}
