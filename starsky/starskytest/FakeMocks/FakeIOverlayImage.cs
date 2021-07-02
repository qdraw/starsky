using System.IO;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn.CreateAnImageCorrupt;

namespace starskytest.FakeMocks
{
	public class FakeIOverlayImage : IOverlayImage
	{
		private readonly IStorage _storage;

		public FakeIOverlayImage(ISelectorStorage selectorStorage)
		{
			_storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		}
		public string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile)
		{
			return new OverlayImage(null,new AppSettings()).FilePathOverlayImage(
				sourceFilePath, profile);
		}

		public string FilePathOverlayImage(string outputParentFullFilePathFolder, string sourceFilePath,
			AppSettingsPublishProfiles profile)
		{
			return new OverlayImage(null,new AppSettings()).FilePathOverlayImage(outputParentFullFilePathFolder,
				sourceFilePath, profile);
		}

		public void ResizeOverlayImageThumbnails(string itemFileHash, string outputFullFilePath,
			AppSettingsPublishProfiles profile)
		{
			ResizeOverlayImageLarge(itemFileHash, outputFullFilePath, profile);
		}

		public void ResizeOverlayImageLarge(string itemFilePath, string outputFullFilePath,
			AppSettingsPublishProfiles profile)
		{
			if ( itemFilePath == "/corrupt.jpg" || itemFilePath == "corrupt")
			{
				_storage.WriteStream(new MemoryStream(new CreateAnImageCorrupt().Bytes),
					outputFullFilePath);
				return;
			}
			
			_storage.WriteStream(new MemoryStream(FakeCreateAn.CreateAnImageNoExif.Bytes),
				outputFullFilePath);
		}
	}
}
