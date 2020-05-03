using System;
using System.IO;
using System.Linq;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
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

		public ThumbnailCleaner(IStorage thumbnailStorage, IQuery iQuery, AppSettings appSettings)
		{
			_thumbnailStorage = thumbnailStorage;
			_appSettings = appSettings;
			_query = iQuery;
		}
	
		public void CleanAllUnusedFiles()
		{
			if (! _thumbnailStorage.ExistFolder("/") ) throw new DirectoryNotFoundException("Thumbnail folder not found");

			var allThumbnailFiles = _thumbnailStorage.GetAllFilesInDirectory("/").ToList();
			if(_appSettings.Verbose) Console.WriteLine(allThumbnailFiles.Count);
			
			foreach ( var thumbnailFile in allThumbnailFiles )
			{
				var fileHash = Path.GetFileNameWithoutExtension(thumbnailFile);
				var itemByHash = _query.GetSubPathByHash(fileHash);
				if (itemByHash != null ) continue;

				_thumbnailStorage.FileDelete(fileHash);
				Console.Write("$");
			}
		}
	}
}
