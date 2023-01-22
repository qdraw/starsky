using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Interfaces;

public interface IThumbnailQuery
{
	Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(List<ThumbnailResultDataTransferModel> thumbnailItems);

	Task<List<ThumbnailItem>> Get(string? fileHash = null);
	Task RemoveThumbnails(List<string> deletedFileHashes);
	Task<bool> RenameAsync(string beforeFileHash, string newFileHash);
	Task<List<ThumbnailItem>> UnprocessedGeneratedThumbnails();

	/// <summary>
	/// Update specific thumbnail item with data
	/// </summary>
	/// <param name="item">the item received</param>
	/// <returns></returns>
	Task<bool> UpdateAsync(ThumbnailItem item);
}
