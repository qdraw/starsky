using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnQuickTimeMp4;

public class CreateAnQuickTimeMp4A6700
{
	public readonly ImmutableArray<byte> Bytes = [];

	public CreateAnQuickTimeMp4A6700()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnQuickTimeMp4", "C0008_head_part.mp4");

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
