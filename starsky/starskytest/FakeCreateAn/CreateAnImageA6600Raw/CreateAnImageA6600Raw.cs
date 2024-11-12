using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageA6600Raw;

public class CreateAnImageA6600Raw
{
	private readonly string _dirName;
	public readonly ImmutableArray<byte> Bytes = [..Array.Empty<byte>()];

	public CreateAnImageA6600Raw()
	{
		_dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
		if ( string.IsNullOrEmpty(_dirName) )
		{
			return;
		}

		var path = Path.Combine(_dirName, "FakeCreateAn",
			"CreateAnImageA6600Raw", "head_part.arw");

		Bytes = [..StreamToBytes(path)];
	}

	public byte[] BytesFullImage
	{
		get
		{
			var path = Path.Combine(_dirName, "FakeCreateAn",
				"CreateAnImageA6600Raw", "20241107_140535_DSC00732.arw");
			return StreamToBytes(path);
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
