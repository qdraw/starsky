using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.Models;

public class GenerationResultModel
{
	public string FileHash { get; set; } = string.Empty;
	public string SubPath { get; set; } = string.Empty;
	public bool Success { get; set; }
	public bool IsNotFound { get; set; }
	public string? ErrorMessage { get; set; } = string.Empty;
	public ThumbnailSize Size { get; set; } = ThumbnailSize.Unknown;

	public int SizeInPixels
	{
		get => ThumbnailNameHelper.GetSize(Size);
		set => Size = ThumbnailNameHelper.GetSize(value);
	}
}
