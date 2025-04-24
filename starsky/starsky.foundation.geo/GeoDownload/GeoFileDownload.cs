using starsky.feature.geolookup.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.geo.GeoDownload
{
	[Service(typeof(IGeoFileDownload), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class GeoFileDownload : IGeoFileDownload
	{
		private readonly AppSettings _appSettings;
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly IStorage _hostStorage;
		private readonly IWebLogger _logger;

		public const string CountryName = "cities1000";
		internal long MinimumSizeInBytes { get; set; } = 7000000; // 7 MB

		public GeoFileDownload(AppSettings appSettings, IHttpClientHelper httpClientHelper,
			ISelectorStorage selectorStorage, IWebLogger logger)
		{
			_appSettings = appSettings;
			_httpClientHelper = httpClientHelper;
			_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_logger = logger;
		}

		internal const string BaseUrl =
			"download.geonames.org/export/dump/";

		internal const string MirrorUrl = "qdraw.nl/special/mirror/geonames/";

		public async Task DownloadAsync()
		{
			RemoveFailedDownload();
			CreateDependenciesFolder();
			const string https = "https://";
			const string admin1CodesasciiTxt = "admin1CodesASCII.txt";

			if ( !_hostStorage.ExistFile(
					Path.Combine(_appSettings.DependenciesFolder, CountryName + ".txt")) )
			{
				var outputZip = Path.Combine(_appSettings.DependenciesFolder,
					CountryName + ".zip");
				var baseResult =
					await _httpClientHelper.Download(https + BaseUrl + CountryName + ".zip",
						outputZip);
				if ( !baseResult )
				{
					await _httpClientHelper.Download(https + MirrorUrl + CountryName + ".zip",
						outputZip);
				}

				new Zipper(_logger).ExtractZip(outputZip, _appSettings.DependenciesFolder);
			}

			if ( !_hostStorage.ExistFile(
					Path.Combine(_appSettings.DependenciesFolder, admin1CodesasciiTxt)) )
			{
				// code for the second administrative division,
				// a county in the US, see file admin2Codes.txt; varchar(80)
				var outputFile = Path.Combine(_appSettings.DependenciesFolder,
					admin1CodesasciiTxt);
				var baseResult = await _httpClientHelper.Download(https +
					BaseUrl + admin1CodesasciiTxt, outputFile);
				if ( !baseResult )
				{
					await _httpClientHelper.Download(https +
													 MirrorUrl + admin1CodesasciiTxt, outputFile);
				}
			}
		}

		internal void CreateDependenciesFolder()
		{
			if ( !_hostStorage.ExistFolder(_appSettings.DependenciesFolder) )
			{
				_hostStorage.CreateDirectory(
					PathHelper.RemoveLatestBackslash(_appSettings.DependenciesFolder)!);
			}
		}

		/// <summary>
		/// Check if the .zip file exist and if its larger then MinimumSizeInBytes
		/// </summary>
		internal void RemoveFailedDownload()
		{
			if ( !_hostStorage.ExistFile(Path.Combine(
					_appSettings.DependenciesFolder,
					CountryName + ".zip")) )
			{
				return;
			}

			// When trying to download a file
			var zipLength = _hostStorage
				.ReadStream(Path.Combine(_appSettings.DependenciesFolder, CountryName + ".zip"))
				.Length;
			if ( zipLength > MinimumSizeInBytes )
			{
				return;
			}

			_hostStorage.FileDelete(Path.Combine(_appSettings.DependenciesFolder,
				CountryName + ".zip"));
		}
	}
}
