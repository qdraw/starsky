using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.ArchiveFormats
{
	[TestClass]
	public class TarBalTest
	{
		private readonly StorageHostFullPathFilesystem _hostStorageProvider;

		public TarBalTest()
		{
			_hostStorageProvider = new StorageHostFullPathFilesystem();
		}
		
		[TestMethod]
		public void ExtractTar()
		{
			// Non Gz Tar
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string>{});

			var memoryStream = new MemoryStream(CreateAnExifToolTar.Bytes);
			new TarBal(storage).ExtractTar(memoryStream,"/test");
			Assert.IsTrue(storage.ExistFile("/test/Image-ExifTool-11.99/exiftool"));
		}
		
		[TestMethod]
		public void ExtractTarGz()
		{
			// Gz Tar!
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string>{});

			var memoryStream = new MemoryStream(CreateAnExifToolTarGz.Bytes);
			new TarBal(storage).ExtractTarGz(memoryStream,"/test");
			Assert.IsTrue(storage.ExistFile("/test/Image-ExifTool-11.99/exiftool"));
		}
	}
}
