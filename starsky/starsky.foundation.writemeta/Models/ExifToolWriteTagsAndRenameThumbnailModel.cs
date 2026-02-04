using System.Collections.Generic;
using starsky.foundation.platform.Thumbnails;

namespace starsky.foundation.writemeta.Models;

public class ExifToolWriteTagsAndRenameThumbnailModel(
	bool isSuccess,
	string newFileHash,
	List<(bool, ThumbnailSize)>? fileMoveResult = null)
{
	public bool IsSuccess { get; set; } = isSuccess;
	public string NewFileHash { get; set; } = newFileHash;
	public List<(bool, ThumbnailSize)>? FileMoveResult { get; set; } = fileMoveResult;

	public override string ToString()
	{
		return $"S: {IsSuccess}, N:{NewFileHash} R: {string.Join(", ", FileMoveResult ?? [])}";
	}
}
