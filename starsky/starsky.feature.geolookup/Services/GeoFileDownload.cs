using System.IO;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Storage;

namespace starsky.feature.geolookup.Services
{
	[Service(typeof(IGeoFileDownload), InjectionLifetime = InjectionLifetime.Singleton)]
	public class GeoFileDownload : IGeoFileDownload
	{
		private readonly AppSettings _appSettings;
		private readonly IHttpClientHelper _httpClientHelper;

		public const string CountryName = "cities1000";
		private const long MinimumSizeInBytes = 7000000; // 7 MB
		
		public GeoFileDownload(AppSettings appSettings, IHttpClientHelper httpClientHelper)
		{
			_appSettings = appSettings;
			_httpClientHelper = httpClientHelper;
		}

		private const string BaseUrl =
			"download.geonames.org/export/dump/";
		private const string MirrorUrl = "qdraw.nl/special/geonames/";
		
		public async Task Download()
		{
			RemoveFailedDownload();
	        
			if(!new StorageHostFullPathFilesystem().ExistFile(
				Path.Combine(_appSettings.TempFolder,CountryName + ".txt")) )
			{
				var outputZip = Path.Combine(_appSettings.TempFolder,
					CountryName + ".zip");
				var baseResult = await _httpClientHelper.Download( "https://" +  BaseUrl + CountryName + ".zip",outputZip);
				if ( !baseResult )
				{
					await _httpClientHelper.Download("https://" + MirrorUrl + CountryName + ".zip",outputZip);
				}
				new Zipper().ExtractZip(outputZip, _appSettings.TempFolder);
			}

			if(!new StorageHostFullPathFilesystem().ExistFile(
				Path.Combine(_appSettings.TempFolder,"admin1CodesASCII.txt")))
			{
				// code for the second administrative division,
				// a county in the US, see file admin2Codes.txt; varchar(80)
				//downloader.DownloadFile("admin1CodesASCII.txt", _appSettings.TempFolder);
				var outputFile = Path.Combine(_appSettings.TempFolder,
					"admin1CodesASCII.txt");
				var baseResult = await _httpClientHelper.Download("https://" +
					BaseUrl + "admin1CodesASCII.txt",outputFile);
				if ( !baseResult )
				{
					await _httpClientHelper.Download("https://" +
						MirrorUrl + "admin1CodesASCII.txt",outputFile);
				}
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
