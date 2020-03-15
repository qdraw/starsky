using System;
using System.IO;
using System.Linq;
using starsky.foundation.query.Interfaces;
using starskycore.Interfaces;
using starskycore.Models;

namespace starsky.foundation.thumbnailgeneration.Services
{
	public class ThumbnailCleaner
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
			var allThumbnailFiles = GetAllThumbnailFiles();
			if(_appSettings.Verbose) Console.WriteLine(allThumbnailFiles.Length);
			
			foreach ( var thumbnailFile in allThumbnailFiles )
			{
				var fileHash = Path.GetFileNameWithoutExtension(thumbnailFile.Name);
				var itemByHash = _query.GetSubPathByHash(fileHash);
				if (itemByHash != null ) continue;

				_thumbnailStorage.FileDelete(thumbnailFile.FullName);
				Console.Write("$");
			}
		}

		public FileInfo[] GetAllThumbnailFiles()
		{
			DirectoryInfo dirInfo = new DirectoryInfo(_appSettings.ThumbnailTempFolder);
			return dirInfo.EnumerateFiles($"*.jpg", SearchOption.TopDirectoryOnly)
				.AsParallel().ToArray();
		}
	}
}
