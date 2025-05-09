using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageCorrupt;

namespace starskytest.FakeMocks;

public class FakeIOverlayImage : IOverlayImage
{
	private readonly IStorage _storage;

	public FakeIOverlayImage(ISelectorStorage selectorStorage)
	{
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	}

	public string FilePathOverlayImage(string sourceFilePath,
		AppSettingsPublishProfiles profile)
	{
		return new OverlayImage(new FakeSelectorStorage(), new AppSettings()).FilePathOverlayImage(
			sourceFilePath, profile);
	}

	public string FilePathOverlayImage(string outputParentFullFilePathFolder,
		string sourceFilePath,
		AppSettingsPublishProfiles profile)
	{
		return new OverlayImage(new FakeSelectorStorage(), new AppSettings()).FilePathOverlayImage(
			outputParentFullFilePathFolder,
			sourceFilePath, profile);
	}

	public async Task<bool> ResizeOverlayImageThumbnails(string itemFileHash,
		string outputFullFilePath,
		AppSettingsPublishProfiles profile)
	{
		return await ResizeOverlayImageLarge(itemFileHash, outputFullFilePath, profile);
	}

	public async Task<bool> ResizeOverlayImageLarge(string itemFilePath,
		string outputFullFilePath,
		AppSettingsPublishProfiles profile)
	{
		if ( itemFilePath == "/corrupt.jpg" || itemFilePath == "corrupt" )
		{
			await _storage.WriteStreamAsync(
				new MemoryStream(new CreateAnImageCorrupt().Bytes.ToArray()),
				outputFullFilePath);
			return true;
		}

		return await _storage.WriteStreamAsync(
			new MemoryStream(CreateAnImageNoExif.Bytes.ToArray()),
			outputFullFilePath);
	}
}
