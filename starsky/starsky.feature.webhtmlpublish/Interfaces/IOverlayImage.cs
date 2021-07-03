using System.Threading.Tasks;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IOverlayImage
	{
		string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile);
		string FilePathOverlayImage(string outputParentFullFilePathFolder,
			string sourceFilePath, AppSettingsPublishProfiles profile);

		Task<bool> ResizeOverlayImageThumbnails(string itemFileHash, string outputFullFilePath, AppSettingsPublishProfiles profile);
		Task<bool> ResizeOverlayImageLarge(string itemFilePath, string outputFullFilePath, AppSettingsPublishProfiles profile);
	}
}
