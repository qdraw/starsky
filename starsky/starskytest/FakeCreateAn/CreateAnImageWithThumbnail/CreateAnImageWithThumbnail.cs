using System;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageWithThumbnail
{
	public class CreateAnImageWithThumbnail
	{
		public CreateAnImageWithThumbnail()
		{
			var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if ( string.IsNullOrEmpty(dirName) ) return;
			var path = Path.Combine(dirName, "FakeCreateAn",
				"CreateAnImageWithThumbnail", "poppy.jpg");

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
