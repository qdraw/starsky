namespace starsky.foundation.storage.Models;

public class FullFilePathExistsResultModel(
	bool? isSuccess = null,
	string? fullFilePath = null,
	bool? isTempFile = null,
	string? fileHashWithExtension = null)
{
	public bool IsSuccess { get; set; } = isSuccess ?? false;
	public string FullFilePath { get; set; } = fullFilePath ?? string.Empty;
	public bool IsTempFile { get; set; } = isTempFile ?? false;
	public string FileHashWithExtension { get; set; } = fileHashWithExtension ?? string.Empty;

	public void Deconstruct(out bool isSuccess, out string fullFilePath, out bool isTempFile,
		out string fileHashWithExtension)
	{
		fullFilePath = FullFilePath;
		isTempFile = IsTempFile;
		fileHashWithExtension = FileHashWithExtension;
		isSuccess = IsSuccess;
	}
}
