using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.writemeta.Models;

namespace starsky.foundation.writemeta.Interfaces;

public interface IExifTool
{
	Task<bool> WriteTagsAsync(string subPath, string command);

	Task<ExifToolWriteTagsAndRenameThumbnailModel> WriteTagsAndRenameThumbnailAsync(
		string subPath,
		string? beforeFileHash, string command,
		CancellationToken cancellationToken = default);

	Task<bool> WriteTagsThumbnailAsync(string fileHash, string command);
}
