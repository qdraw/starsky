using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Services
{
	[Service(typeof(IThumbnailCleaner), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ThumbnailCleaner : IThumbnailCleaner
	{
		private readonly AppSettings _appSettings;
		private readonly IQuery _query;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;

		public ThumbnailCleaner(IStorage thumbnailStorage, IQuery iQuery, AppSettings appSettings, IWebLogger logger)
		{
			_thumbnailStorage = thumbnailStorage;
			_appSettings = appSettings;
			_query = iQuery;
			_logger = logger;
		}
	
		[Obsolete("use CleanAllUnusedFilesAsync")]
		public void CleanAllUnusedFiles()
		{
			if ( !_thumbnailStorage.ExistFolder("/") )
			{
				throw new DirectoryNotFoundException("Thumbnail folder not found");
			}

			var allThumbnailFiles = _thumbnailStorage.GetAllFilesInDirectory(null).ToList();
			if(_appSettings.IsVerbose()) _logger.LogInformation($"Total files in thumb dir: {allThumbnailFiles.Count}");
			
			foreach ( var thumbnailFile in allThumbnailFiles )
			{
				var fileHash = Path.GetFileNameWithoutExtension(thumbnailFile);
				var fileHashWithoutSize = Regex.Match(fileHash, "^.*(?=(@))").Value;
				if ( string.IsNullOrEmpty(fileHashWithoutSize)  )
				{
					fileHashWithoutSize = fileHash;
				}

				try
				{
					var itemByHash = _query.GetSubPathByHash(fileHashWithoutSize);
					if (itemByHash != null ) continue;
				}
				catch (Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException exception)
				{
					_logger.LogInformation($"[CleanAllUnusedFiles] catch-ed and " +
					                       $"skip {fileHash} ~ {exception.Message}", exception);
					continue;
				}

				_thumbnailStorage.FileDelete(fileHash);
				Console.Write("$");
			}
		}

		public async Task<List<string>> CleanAllUnusedFilesAsync(int chunkSize = 50)
		{
			if ( !_thumbnailStorage.ExistFolder("/") )
			{
				throw new DirectoryNotFoundException("Thumbnail folder not found");
			}

			var allThumbnailFiles = _thumbnailStorage
				.GetAllFilesInDirectory(null).ToList();
			_logger.LogDebug($"Total files in thumb dir: {allThumbnailFiles.Count}");

			var deletedFileHashes = new List<string>();
			foreach ( var fileNamesInChunk in allThumbnailFiles.Chunk(chunkSize) )
			{
				var itemsInChunk = GetFileNamesWithExtension(fileNamesInChunk.ToList());
				try
				{
					await LoopThoughChunk(itemsInChunk, deletedFileHashes);
				}
				catch (Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException exception)
				{
					_logger.LogInformation($"[CleanAllUnusedFiles] catch-ed and " +
					                       $"skip {string.Join(",", itemsInChunk.ToList())} ~ {exception.Message}", exception);
				}
			}
			return deletedFileHashes;
		}

		private async Task LoopThoughChunk(IEnumerable<string> itemsInChunk , List<string> deletedFileHashes)
		{
			var fileIndexItems = await _query.GetObjectsByFileHashAsync(itemsInChunk.ToList());
			foreach ( var result in fileIndexItems.Where(result => 
				result.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex
				))
			{
				var fileHashesToDelete = new List<string>
				{
					ThumbnailNameHelper.Combine(result.FileHash,
						ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine(result.FileHash,
						ThumbnailSize.ExtraLarge),
					ThumbnailNameHelper.Combine(result.FileHash,
						ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine(result.FileHash,
						ThumbnailSize.Large)
				};
				foreach ( var fileHash in fileHashesToDelete )
				{
					_thumbnailStorage.FileDelete(fileHash);
				}
				
				_logger.LogInformation("$");
				deletedFileHashes.Add(result.FileHash);
			}
		}

		private HashSet<string> GetFileNamesWithExtension(List<string> allThumbnailFiles)
		{
			var results = new List<string>();
			foreach ( var thumbnailFile in allThumbnailFiles )
			{
				var fileHash = Path.GetFileNameWithoutExtension(thumbnailFile);
				var fileHashWithoutSize = Regex.Match(fileHash, "^.*(?=(@))").Value;
				if ( string.IsNullOrEmpty(fileHashWithoutSize)  )
				{
					fileHashWithoutSize = fileHash;
				}
				results.Add(fileHashWithoutSize);
			}
			return new HashSet<string>(results);
		}
	}
}
