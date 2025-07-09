using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview.Models;

public class NativePreviewResult
{
	public bool IsSuccess { get; set; }
	public bool ErrorLog { get; set; }
	public string? ResultPath { get; set; }
	public string ErrorMessage { get; set; } = string.Empty;
	public SelectorStorage.StorageServices? ResultPathType { get; set; }
}
