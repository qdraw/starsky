using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

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
			return "";
		}

		public string FilePathOverlayImage(string outputParentFullFilePathFolder, string sourceFilePath,
			AppSettingsPublishProfiles profile)
		{
			return "";
		}

		public void ResizeOverlayImageThumbnails(string itemFileHash, string outputFullFilePath,
			AppSettingsPublishProfiles profile)
		{
			_storage.WriteStream(new PlainTextFileHelper().StringToStream("not 0 bytes"),
				outputFullFilePath);
		}

		public void ResizeOverlayImageLarge(string itemFilePath, string outputFullFilePath,
			AppSettingsPublishProfiles profile)
		{
			_storage.WriteStream(new PlainTextFileHelper().StringToStream("not 0 bytes"),
				outputFullFilePath);
		}
	}
}
