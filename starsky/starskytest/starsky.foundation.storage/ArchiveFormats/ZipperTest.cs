using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starskytest.FakeCreateAn.CreateAnZipFile12;

namespace starskytest.starsky.foundation.storage.ArchiveFormats
{
	[TestClass]
	public sealed class ZipperTest
	{
		[TestMethod]
		public void NotFound()
		{
			var result =  new Zipper().ExtractZip("not-found","t");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void TestExtractZip()
		{
			// Arrange
			var zipped = CreateAnZipFile12.Bytes;

			// Act
			var result = Zipper.ExtractZip(zipped);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(CreateAnZipFile12.Content.Count, result.Count);

			foreach (var entry in CreateAnZipFile12.Content)
			{
				Assert.IsTrue(result.ContainsKey(entry.Key));
				var resultText = Encoding.UTF8.GetString(result[entry.Key]);
				Assert.AreEqual(entry.Value, resultText);
			}
		}
	}
}
