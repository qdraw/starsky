using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Services
{
	[Service(typeof(IThumbnailCleaner), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class ThumbnailCleaner : IThumbnailCleaner
	{
		private readonly IQuery _query;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;
		private readonly IThumbnailQuery _thumbnailQuery;

		public ThumbnailCleaner(IStorage thumbnailStorage, IQuery iQuery, IWebLogger logger, IThumbnailQuery thumbnailQuery)
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
				catch ( Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException exception )
				{
					_logger.LogInformation($"[CleanAllUnusedFiles] catch-ed and " +
										   $"skip {string.Join(",", itemsInChunk.ToList())} ~ {exception.Message}", exception);
				}
			}

			await _thumbnailQuery.RemoveThumbnailsAsync(deletedFileHashes);
			return deletedFileHashes;
		}

		private async Task LoopThoughChunk(IEnumerable<string> itemsInChunk, List<string> deletedFileHashes)
		{
			var fileIndexItems = await _query.GetObjectsByFileHashAsync(itemsInChunk.ToList());
			foreach ( var resultFileHash in fileIndexItems.Where(result =>
				result is { Status: FileIndexItem.ExifStatus.NotFoundNotInIndex, FileHash: { } }
				).Select(p => p.FileHash).Cast<string>() )
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

		[SuppressMessage("Performance", "CA1822:Mark members as static")]
		[SuppressMessage("ReSharper", "S2325: Static property")]
		// ReSharper disable once MemberCanBeMadeStatic.Global
		private HashSet<string> GetFileNamesWithExtension(List<string> allThumbnailFiles)
		{
			var results = new List<string>();
			foreach ( var thumbnailFile in allThumbnailFiles )
			{
				var fileHash = Path.GetFileNameWithoutExtension(thumbnailFile);
				var fileHashWithoutSize = Regex.Match(fileHash, "^.*(?=(@))",
					RegexOptions.None, TimeSpan.FromMilliseconds(100)).Value;
				if ( string.IsNullOrEmpty(fileHashWithoutSize) )
				{
					fileHashWithoutSize = fileHash;
				}
				results.Add(fileHashWithoutSize);
			}
			return new HashSet<string>(results);
		}
	}
}
