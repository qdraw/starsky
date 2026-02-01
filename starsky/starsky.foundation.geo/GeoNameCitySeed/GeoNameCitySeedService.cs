using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.GeoNamesCities.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.geo.GeoDownload;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.geo.GeoRegionInfo;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.geo.GeoNameCitySeed;

[Service(typeof(IGeoNameCitySeedService), InjectionLifetime = InjectionLifetime.Scoped)]
public class GeoNameCitySeedService(
	ISelectorStorage selectorStorage,
	AppSettings appSettings,
	IGeoFileDownload geoFileDownload,
	IGeoNamesCitiesQuery query,
	IWebLogger logger,
	IMemoryCache? memoryCache)
	: IGeoNameCitySeedService
{
	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	private Dictionary<string, string>? _admin1TextAsciiMap;

	public async Task<bool> Seed()
	{
		if ( await Setup() )
		{
			return true;
		}

		var stopWatch = StopWatchLogger.StartUpdateReplaceStopWatch();

		var batch = new List<GeoNameCity>(50);
		var seenIds = new HashSet<int>();
		await foreach ( var line in _hostStorage.ReadLinesAsync(GeoCountryNamesPath(),
			               CancellationToken.None) )
		{
			var city = await ParseCityAsync(line);
			if ( !seenIds.Add(city.GeonameId) )
			{
				continue;
			}

			batch.Add(city);
			if ( batch.Count < 50 )
			{
				continue;
			}

			await query.AddRange(batch);
			batch.Clear();
		}

		logger.LogInformation("Seeded GeoNameCity batch finalizing...");
		if ( batch.Count > 0 )
		{
			await query.AddRange(batch);
		}

		new StopWatchLogger(logger).StopUpdateReplaceStopWatch(
			nameof(GeoNameCitySeedService), "Seed", true, stopWatch);

		return true;
	}

	private string GeoCountryNamesPath()
	{
		return Path.Combine(appSettings.DependenciesFolder, $"{GeoFileDownload.CountryName}.txt");
	}

	private string GeoAdmin1CodesAsciiNamesPath()
	{
		return Path.Combine(appSettings.DependenciesFolder,
			$"{GeoFileDownload.Admin1CodesAscii}.txt");
	}

	internal async Task<bool> Setup()
	{
		// true is skip import
		const string cacheKey = "GeoNameCitySeedService.Setup";
		if ( memoryCache?.TryGetValue(cacheKey, out bool cached)
		    != null && cached )
		{
			return true;
		}

		if ( appSettings.GeoFilesSkipDownloadOnStartup != true )
		{
			await geoFileDownload.DownloadAsync();
		}

		if ( !_hostStorage.ExistFile(GeoCountryNamesPath()) )
		{
			return true;
		}

		var shouldSkipImport = !await CheckIfFirstLineExists();
		if ( shouldSkipImport )
		{
			memoryCache?.Set(cacheKey, true);
		}

		return shouldSkipImport;
	}

	private async Task<bool> CheckIfFirstLineExists()
	{
		var firstLine = string.Empty;
		await foreach ( var line in _hostStorage.ReadLinesAsync(GeoCountryNamesPath(),
			               CancellationToken.None) )
		{
			if ( string.IsNullOrEmpty(line) )
			{
				continue;
			}

			firstLine = line;
			break;
		}

		var geoNameCity = await ParseCityAsync(firstLine);
		return await query.GetItem(geoNameCity.GeonameId) == null;
	}

	public async Task<string> GetProvince(string p8, string p10)
	{
		var admin1TextAsciiMap = await GetAdmin1TextAsciiMapAsync();
		var admin1Code = p8 + "." + p10;

		return admin1TextAsciiMap.TryGetValue(admin1Code, out var provinceName)
			? provinceName
			: string.Empty;
	}

	private async Task<Dictionary<string, string>> GetAdmin1TextAsciiMapAsync()
	{
		if ( _admin1TextAsciiMap != null )
		{
			return _admin1TextAsciiMap;
		}

		var map = new Dictionary<string, string>();
		if ( !_hostStorage.ExistFile(GeoAdmin1CodesAsciiNamesPath()) )
		{
			return map;
		}

		await foreach ( var line in _hostStorage.ReadLinesAsync(GeoAdmin1CodesAsciiNamesPath(),
			               CancellationToken.None) )
		{
			if ( string.IsNullOrWhiteSpace(line) )
			{
				continue;
			}

			var parts = line.Split('\t');
			if ( parts.Length < 2 )
			{
				continue;
			}

			map[parts[0]] = parts[1];
		}

		_admin1TextAsciiMap = map;
		return map;
	}

	internal async Task<GeoNameCity> ParseCityAsync(string line)
	{
		var p = line.Split('\t');
		if ( p.Length < 19 )
		{
			throw new FormatException("Invalid GeoNames line");
		}
		var (countryNameEnglish, threeLetterLocationCountryCode) =
			new RegionInfoHelper(logger).GetLocationCountryAndCode(p[8]);

		return new GeoNameCity
		{
			GeonameId = int.Parse(p[0]),
			Name = p[1],
			AsciiName = p[2],
			AlternateNames = p[3],
			Latitude = double.Parse(p[4], CultureInfo.InvariantCulture),
			Longitude = double.Parse(p[5], CultureInfo.InvariantCulture),
			FeatureClass = p[6],
			FeatureCode = p[7],
			CountryCode = p[8],
			Cc2 = p[9],
			Admin1Code = p[10],
			Admin2Code = p[11],
			Admin3Code = p[12],
			Admin4Code = p[13],
			Population = long.Parse(p[14]),
			Elevation = string.IsNullOrEmpty(p[15]) ? null : int.Parse(p[15]),
			Dem = int.Parse(p[16]),
			TimeZoneId = p[17],
			ModificationDate =
				DateOnly.ParseExact(p[18], "yyyy-MM-dd", CultureInfo.InvariantCulture),
			Province = await GetProvince(p[8], p[10]),
			CountryName = countryNameEnglish,
			CountryThreeLetterCode = threeLetterLocationCountryCode
		};
	}
}
