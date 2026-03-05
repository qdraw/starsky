namespace starsky.feature.webftppublish.Models;

public class ExtractZipResultModel
{
	public bool RemoveFolderAfterwards { get; set; }
	public string FullFileFolderPath { get; set; } = string.Empty;
	public bool IsError { get; set; }

	public bool IsFromZipFile { get; set; }
}
