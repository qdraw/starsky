using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageEOSM50Raw;

public class CreateAnImageEOSM50Raw
{
	public readonly ImmutableArray<byte> Bytes = [];

	public CreateAnImageEOSM50Raw()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		// source: https://github.com/lclevy/canon_cr3/blob/master/samples/IMG_0483_craw.CR3
		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageEOSM50Raw", "IMG_0483_Canon EOS M50.CR3");

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
