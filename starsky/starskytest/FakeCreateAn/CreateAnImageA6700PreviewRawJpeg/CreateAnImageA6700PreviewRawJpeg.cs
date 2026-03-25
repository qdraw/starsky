using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageA6700PreviewRawJpeg;

public class CreateAnImageA6700PreviewRawJpeg
{
	public readonly ImmutableArray<byte> Bytes = [];
	public readonly ImmutableArray<byte> BytesJpeg = [];

	public CreateAnImageA6700PreviewRawJpeg()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageA6700PreviewRawJpeg", "20260308_144118_DSC05172.arw");

		Bytes = [..StreamToBytes(path)];

		var pathJpg = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageA6700PreviewRawJpeg", "20260308_144118_DSC05172.jpg");

		FilePathJpeg = pathJpg;
		BytesJpeg = [..StreamToBytes(pathJpg)];
	}

	public string FilePathJpeg { get; set; } = null!;

	private static byte[] StreamToBytes(string path)
	{
		var input = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
		using var ms = new MemoryStream();
		input.CopyTo(ms);
		input.Dispose();
		return ms.ToArray();
	}
}
