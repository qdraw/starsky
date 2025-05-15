using starsky.foundation.native.PreviewImageNative;
using starsky.foundation.native.PreviewImageNative.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIPreviewImageNativeService : IPreviewImageNativeService
{
	private readonly IStorage _storage;

	public FakeIPreviewImageNativeService(IStorage? storage = null, bool isSupported = true)
	{
		IsSupportedResult = isSupported;
		_storage = storage ?? new FakeIStorage();
	}

	public bool IsSupportedResult { get; set; }

	public bool IsSupported(int width = 512)
	{
		return IsSupportedResult;
	}

	public string FileExtension()
	{
		return new PreviewImageNativeService(new FakeIWebLogger()).FileExtension();
	}

	public bool GeneratePreviewImage(string filePath, string outputPath, int width, int height)
	{
		_storage.FileCopy(filePath, outputPath);
		_storage.FileCopy(filePath,
			new AppSettings().FullPathTempFolderToDatabaseStyle(outputPath));
		_storage.FileCopy(filePath, FilenamesHelper.GetFileName(outputPath));

		return IsSupportedResult;
	}
}
