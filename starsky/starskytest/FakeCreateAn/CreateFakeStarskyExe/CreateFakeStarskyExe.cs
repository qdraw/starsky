using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateFakeStarskyExe
{
	public class CreateFakeStarskyExe
	{
		public CreateFakeStarskyExe()
		{
			var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if ( string.IsNullOrEmpty(dirName) ) return;
			var parentFolder = Path.Combine(dirName, "FakeCreateAn",
				"CreateFakeStarskyExe");
			var path = Path.Combine(parentFolder, "starsky.exe");
			FullFilePath = path;
			StarskyDotStarskyPath = Path.Combine(parentFolder, "starsky.starsky");

			Bytes = [.. StreamToBytes(path)];
		}

		public string FullFilePath { get; set; } = string.Empty;
		public string StarskyDotStarskyPath { get; set; } = string.Empty;

		private static byte[] StreamToBytes(string path)
		{
			var input = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			input.Dispose();
			return ms.ToArray();
		}

		public readonly ImmutableArray<byte> Bytes = [];

	}
}

