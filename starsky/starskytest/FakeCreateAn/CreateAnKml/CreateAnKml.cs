using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnKml;

public class CreateAnKml
{
	public CreateAnKml()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) ) return;
		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnKml", "garmin.kml");

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

	public readonly byte[] Bytes;
}
