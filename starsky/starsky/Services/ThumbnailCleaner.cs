using System;
using System.IO;
using System.Linq;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
	public class ThumbnailCleaner
	{
		private readonly AppSettings _appSettings;
		private readonly IQuery _query;

		public ThumbnailCleaner(IQuery iquery, AppSettings appSettings)
		{
			_appSettings = appSettings;
			_query = iquery;

			if (!Directory.Exists(_appSettings.ThumbnailTempFolder))
			{
				throw new DirectoryNotFoundException("ThumbnailTempFolder not found " 
				                                + _appSettings.ThumbnailTempFolder);
			}
		}

	
		public void CleanAllUnusedFiles()
		{

			var allThumbnailFiles = GetAllThumbnailFiles();
			if(_appSettings.Verbose) Console.WriteLine(allThumbnailFiles.Length);
			
			foreach ( var thumbnailFile in allThumbnailFiles )
			{
				var fileHash = Path.GetFileNameWithoutExtension(thumbnailFile.Name);
				var itemByHash = _query.GetItemByHash(fileHash);
				if (itemByHash != null ) continue;

				Files.DeleteFile(thumbnailFile.FullName);
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
