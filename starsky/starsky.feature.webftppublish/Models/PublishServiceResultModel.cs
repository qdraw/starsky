namespace starsky.feature.webftppublish.Models;

public class PublishServiceResultModel(
	bool isSuccess,
	string? error = null,
	string? zipTempPath = null)
{
	public bool IsSuccess { get; set; } = isSuccess;
	public string Error { get; set; } = error ?? string.Empty;
	public string ParentDirectoryOrZipFile { get; set; } = zipTempPath ?? string.Empty;
}
