using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Services;

[Service(typeof(IThumbnailCleaner), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ThumbnailCleaner : IThumbnailCleaner
{
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IThumbnailQuery _thumbnailQuery;
	private readonly IStorage _thumbnailStorage;

	public ThumbnailCleaner(IStorage thumbnailStorage, IQuery iQuery, IWebLogger logger,
		IThumbnailQuery thumbnailQuery)
	{
		_thumbnailStorage = thumbnailStorage;
		_query = iQuery;
		_logger = logger;
		_thumbnailQuery = thumbnailQuery;
	}

	public async Task<List<string>> CleanAllUnusedFilesAsync(int chunkSize = 50)
	{
		if ( !_thumbnailStorage.ExistFolder("/") )
		{
			throw new DirectoryNotFoundException("Thumbnail folder not found");
		}

		var allThumbnailFiles = _thumbnailStorage
			.GetAllFilesInDirectory(null!).ToList();

		_logger.LogDebug($"Total files in thumb dir: {allThumbnailFiles.Count}");

		var deletedFileHashes = new List<string>();
		foreach ( var fileNamesInChunk in allThumbnailFiles.ChunkyEnumerable(chunkSize) )
		{
			var itemsInChunk = GetFileNamesWithExtension(fileNamesInChunk.ToList());
			try
			{
				await LoopThoughChunk(itemsInChunk, deletedFileHashes);
			}
			catch ( RetryLimitExceededException exception )
			{
				_logger.LogInformation($"[CleanAllUnusedFiles] catch-ed and " +
				                       $"skip {string.Join(",", itemsInChunk.ToList())} ~ {exception.Message}",
					exception);
			}
		}

		await _thumbnailQuery.RemoveThumbnailsAsync(deletedFileHashes);
		return deletedFileHashes;
	}

	private async Task LoopThoughChunk(IEnumerable<string> itemsInChunk,
		List<string> deletedFileHashes)
	{
		var fileIndexItems = await _query
			.GetObjectsByFileHashAsync(itemsInChunk.ToList());
		var hashList = fileIndexItems.Where(result =>
			result is
			{
				Status: FileIndexItem.ExifStatus.NotFoundNotInIndex,
				FileHash: not null
			}
		).Select(p => p.FileHash).Cast<string>();

		foreach ( var resultFileHash in hashList )
		{
			var fileHashesToDelete = new List<string>
			{
				ThumbnailNameHelper.Combine(resultFileHash,
					ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine(resultFileHash,
					ThumbnailSize.ExtraLarge),
				ThumbnailNameHelper.Combine(resultFileHash,
					ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine(resultFileHash,
					ThumbnailSize.Large)
			};

			foreach ( var fileHash in fileHashesToDelete )
			{
				_thumbnailStorage.FileDelete(fileHash);
			}

			_logger.LogInformation("$");
			deletedFileHashes.Add(resultFileHash);
		}
	}

	internal static HashSet<string> GetFileNamesWithExtension(List<string> allThumbnailFiles)
	{
		var results = new List<string>();
		foreach ( var thumbnailFile in allThumbnailFiles )
		{
			var fileHash = Path.GetFileNameWithoutExtension(thumbnailFile);
			var fileHashWithoutSize = GetFileHashWithoutSize(fileHash);
			results.Add(fileHashWithoutSize);
		}

		return [..results];
	}

	internal static string GetFileHashWithoutSize(string fileHash)
	{
		var atIndex = fileHash.IndexOf('@');
		var fileHashWithoutSize = atIndex >= 0 ? fileHash.Substring(0, atIndex) : fileHash;
		return fileHashWithoutSize;
	}
}
