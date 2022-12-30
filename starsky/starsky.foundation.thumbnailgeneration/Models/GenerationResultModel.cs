using starsky.foundation.platform.Enums;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.Models;

public class GenerationResultModel
{
	public string FileHash { get; set; }
	public string SubPath { get; set; }
	public bool Success { get; set; }
	public bool IsNotFound { get; set; } = false;
	public string? ErrorMessage { get; set; } = string.Empty;
	public ThumbnailSize Size { get; set; } = ThumbnailSize.Unknown;

	public int SizeInPixels
	{
		set => Size = ThumbnailNameHelper.GetSize(value);
	}
}
