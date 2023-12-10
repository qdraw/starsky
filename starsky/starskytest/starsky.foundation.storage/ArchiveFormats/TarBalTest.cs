using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnTagGzLongerThan100CharsFileName;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.ArchiveFormats
{
	[TestClass]
	public sealed class TarBalTest
	{
		private readonly StorageHostFullPathFilesystem _hostStorageProvider;

		public TarBalTest()
		{
			_hostStorageProvider = new StorageHostFullPathFilesystem();
		}
		
		[TestMethod]
		public async Task ExtractTar()
		{
			// Non Gz Tar
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string>{});

			var memoryStream = new MemoryStream(CreateAnExifToolTar.Bytes.ToArray());
			await new TarBal(storage).ExtractTar(memoryStream,"/test", CancellationToken.None);
			Assert.IsTrue(storage.ExistFile("/test/Image-ExifTool-11.99/exiftool"));
		}
		
		[TestMethod]
		public async Task ExtractTarGz()
		{
			// Gz Tar!
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string>{});

			var memoryStream = new MemoryStream(CreateAnExifToolTarGz.Bytes.ToArray());
			await new TarBal(storage).ExtractTarGz(memoryStream,"/test", CancellationToken.None);
			Assert.IsTrue(storage.ExistFile("/test/Image-ExifTool-11.99/exiftool"));
		}


		[TestMethod]
		public async Task ExtractTarGz_LongerThan100Chars()
		{
			// Gz Tar!
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string>{});

			var memoryStream = new MemoryStream(new CreateAnTagGzLongerThan100CharsFileName().Bytes);
			await new TarBal(storage).ExtractTarGz(memoryStream,"/test", CancellationToken.None);
			Assert.IsTrue(storage.ExistFile($"/test/{CreateAnTagGzLongerThan100CharsFileName.FileName}"));
			var file = storage.ReadStream($"/test/{CreateAnTagGzLongerThan100CharsFileName.FileName}");
			// the filename is written as content in the file
			Assert.AreEqual(CreateAnTagGzLongerThan100CharsFileName.FileName,PlainTextFileHelper.StreamToString(file).Trim());
		}
	}
}
