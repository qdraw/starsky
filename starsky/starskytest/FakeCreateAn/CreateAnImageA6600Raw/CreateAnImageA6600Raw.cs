using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageA6600Raw;

public class CreateAnImageA6600Raw
{
	public readonly ImmutableArray<byte> Bytes = [];

	public CreateAnImageA6600Raw()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageA6600Raw", "20241107_140535_DSC00732.arw");

		Bytes = [..StreamToBytes(path)];
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
