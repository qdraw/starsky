using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IOverlayImage
	{
		string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile);
		public string FilePathOverlayImage(string outputParentFullFilePathFolder,
			string sourceFilePath, AppSettingsPublishProfiles profile);

		void ResizeOverlayImageThumbnails(string itemFileHash, string outputPath, AppSettingsPublishProfiles profile);
		void ResizeOverlayImageLarge(string itemFilePath, string outputPath, AppSettingsPublishProfiles profile);
	}
}
