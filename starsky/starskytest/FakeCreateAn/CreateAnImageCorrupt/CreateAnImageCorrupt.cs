using System;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageCorrupt
{
	public class CreateAnImageCorrupt
	{
		public CreateAnImageCorrupt()
		{
			var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if ( string.IsNullOrEmpty(dirName) ) return;
			var path = Path.Combine(dirName, "FakeCreateAn",
				"CreateAnImageCorrupt", "corrupt.jpg");

			Bytes = StreamToBytes(path);
		}

		private byte[] StreamToBytes(string path)
		{
			using var input = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			input.Dispose();
			return ms.ToArray();
		}

		public readonly byte[] Bytes;

	}
}

