using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageLargePreview;

public class CreateAnImageLargePreview
{
	private readonly string _dirName;

	public CreateAnImageLargePreview()
	{
		_dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
	}

	public byte[] BytesFullImage
	{
		get
		{
			var path = Path.Combine(_dirName, "FakeCreateAn",
				"CreateAnImageLargePreview", "20241112_110839_DSC02741.jpg");
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
