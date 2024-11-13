using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageA330Raw;

public class CreateAnImageA330Raw
{
	private readonly string _dirName;
	public readonly ImmutableArray<byte> Bytes = [..Array.Empty<byte>()];

	public CreateAnImageA330Raw()
	{
		_dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
		if ( string.IsNullOrEmpty(_dirName) )
		{
			return;
		}

		var path = Path.Combine(_dirName, "FakeCreateAn",
			"CreateAnImageA330Raw", "head_part.arw");

		Bytes = [..StreamToBytes(path)];
	}

	public byte[] BytesFullImage
	{
		get
		{
			var path = Path.Combine(_dirName, "FakeCreateAn",
				"CreateAnImageA330Raw", "DSC01028.ARW");
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
