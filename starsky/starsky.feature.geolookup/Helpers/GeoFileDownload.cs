using System.IO;
using NGeoNames;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.geolookup.Helpers
{
	public class GeoFileDownload
	{
		private readonly AppSettings _appSettings;
		
		public const string CountryName = "cities1000";
		private const long MinimumSizeInBytes = 7000000; // 7 MB
		
		public GeoFileDownload(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		public void Download()
		{
			var downloader = GeoFileDownloader.CreateGeoFileDownloader();

			RemoveFailedDownload();
	        
			if(!new StorageHostFullPathFilesystem().ExistFile(Path.Combine(_appSettings.TempFolder,CountryName + ".txt")) )
			{
				downloader.DownloadFile(CountryName + ".zip", _appSettings.TempFolder);    
				// Zip file will be automatically extracted
			}

			if(!new StorageHostFullPathFilesystem().ExistFile(Path.Combine(_appSettings.TempFolder,"admin1CodesASCII.txt")))
			{
				// code for the second administrative division, a county in the US, see file admin2Codes.txt; varchar(80)
				downloader.DownloadFile("admin1CodesASCII.txt", _appSettings.TempFolder);
			}
		}
		
		/// <summary>
		/// Check if the .zip file exist and if its larger then MinimumSizeInBytes
		/// </summary>
		private void RemoveFailedDownload()
		{
			if ( !new StorageHostFullPathFilesystem().ExistFile(Path.Combine(_appSettings.TempFolder,
				CountryName + ".zip")) ) return;
	        
			// When trying to download a file
			var zipLength = new StorageHostFullPathFilesystem()
				.ReadStream(Path.Combine(_appSettings.TempFolder, CountryName + ".zip"))
				.Length;
			if ( zipLength > MinimumSizeInBytes ) return;
			new StorageHostFullPathFilesystem().FileDelete(Path.Combine(_appSettings.TempFolder,
				CountryName + ".zip"));
		}
	}
}
