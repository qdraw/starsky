using System;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnTagGzLongerThan100CharsFileName
{
	/// <summary>
	/// tar -czvf  long.tar.gz 0vs1ontl39mjughoz44odh6mlx5z4k2n0pv7xn43fca79lbphy0vs1ontl39mjughoz44odh6mlx5z4k2n0pv7xn43fca79lbphy.txt
	/// </summary>
	public class CreateAnTagGzLongerThan100CharsFileName
	{
		public CreateAnTagGzLongerThan100CharsFileName()
		{
			var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if ( string.IsNullOrEmpty(dirName) )
			{
				return;
			}

			var path = Path.Combine(dirName, "FakeCreateAn",
				"CreateAnTagGzLongerThan100CharsFileName", "longer_than_100_chars_linux.tar.gz");

			Bytes = StreamToBytes(path);
		}

		private static byte[] StreamToBytes(string path)
		{
			var input = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			input.Dispose();
			return ms.ToArray();
		}

		public readonly byte[] Bytes = Array.Empty<byte>();

		public const string FileName = "0vs1ontl39mjughoz44odh6mlx5z4k2n0pv7xn43fca79lbphy0vs1ontl39mjughoz44odh6mlx5z4k2n0pv7xn43fca79lbphy.txt";
	}
}
