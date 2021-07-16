using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
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
	}
}
