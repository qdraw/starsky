using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageA330Raw;

public class CreateAnImageA330Raw
{
	public readonly ImmutableArray<byte> Bytes = [..Array.Empty<byte>()];

	public CreateAnImageA330Raw()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageA330Raw", "head_part.arw");

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
