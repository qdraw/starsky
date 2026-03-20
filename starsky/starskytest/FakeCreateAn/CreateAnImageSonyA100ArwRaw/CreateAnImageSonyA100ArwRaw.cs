using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageSonyA100ArwRaw;

public class CreateAnImageSonyA100ArwRaw
{
	public readonly ImmutableArray<byte> Bytes = [];

	public CreateAnImageSonyA100ArwRaw()
	{
		if ( string.IsNullOrEmpty(FullFilePath) || !File.Exists(FullFilePath) )
		{
			throw new FileNotFoundException(FullFilePath);
		}

		Bytes = [..StreamToBytes(FullFilePath)];
	}

	public string FullFilePath
	{
		get
		{
			var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if ( string.IsNullOrEmpty(dirName) )
			{
				return string.Empty;
			}

			return Path.Combine(dirName, "FakeCreateAn",
				"CreateAnImageSonyA100ArwRaw", "RAW_SONY_A100.ARW");
		}
	}

	private static byte[] StreamToBytes(string path)
	{
		var input = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
		using var ms = new MemoryStream();
		input.CopyTo(ms);
		input.Dispose();
		return ms.ToArray();
	}
}
