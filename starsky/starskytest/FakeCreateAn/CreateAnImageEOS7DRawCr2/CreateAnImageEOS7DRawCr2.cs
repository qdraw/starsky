using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageEOS7DRawCr2;

public class CreateAnImageEOS7DRawCr2
{
	public readonly ImmutableArray<byte> Bytes = [];

	public CreateAnImageEOS7DRawCr2()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		// source: https://raw.pixls.us/getfile.php/129/nice/Canon%20-%20EOS%207D%20-%20sRAW2%20(sRAW)%20(3:2).CR2
		// https://raw.pixls.us/getfile.php/129/exif/RAW_CANON_EOS_7D-sraw.CR2.exif.txt
		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageEOS7DRawCr2", "Canon - EOS 7D - sRAW2 (sRAW) (3_2).CR2");

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
