using System.Globalization;
using starsky.foundation.database.GeoNamesCities.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.geo.GeoDownload;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.geo.GeoNameCitySeed;

[Service(typeof(IGeoNameCitySeedService), InjectionLifetime = InjectionLifetime.Scoped)]
public class GeoNameCitySeedService : IGeoNameCitySeedService
{
	private readonly IStorage _hostStorage;
	private readonly AppSettings _appSettings;
	private readonly IGeoFileDownload _geoFileDownload;
	private readonly IGeoNamesCitiesQuery _query;

	private string GeoNamesPath()
	{
		return Path.Combine(_appSettings.DependenciesFolder, $"{GeoFileDownload.CountryName}.txt");
	}

	public GeoNameCitySeedService(ISelectorStorage selectorStorage,
		AppSettings appSettings, IGeoFileDownload geoFileDownload, IGeoNamesCitiesQuery query)
	{
		_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_appSettings = appSettings;
		_geoFileDownload = geoFileDownload;
		_query = query;
	}

	private async Task<bool> Setup()
	{
		if ( _appSettings.GeoFilesSkipDownloadOnStartup != true )
		{
			await _geoFileDownload.DownloadAsync();
		}

		if ( _hostStorage.ExistFile(GeoNamesPath()) )
		{
			return await CheckIfFirstLineExists();
		}

		return false;
	}

	public async Task<bool> Seed()
	{
		if ( !await Setup() )
		{
			return true;
		}


		await foreach ( var line in _hostStorage.ReadLinesAsync(GeoNamesPath(),
			               CancellationToken.None) )
		{
			var city = ParseCity(line);
			await _query.AddItem(city);
		}

		return true;
	}

	private async Task<bool> CheckIfFirstLineExists()
	{
		var firstLine = string.Empty;
		await foreach ( var line in _hostStorage.ReadLinesAsync(GeoNamesPath(),
			               CancellationToken.None) )
		{
			if ( string.IsNullOrEmpty(line) )
			{
				continue;
			}

			firstLine = line;
			break;
		}

		var geoNameCity = ParseCity(firstLine);
		return await _query.GetItem(geoNameCity.GeonameId) == null;
	}

	internal static GeoNameCity ParseCity(string line)
	{
		var p = line.Split('\t');

		if ( p.Length < 19 )
		{
			throw new FormatException("Invalid GeoNames line");
		}

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
			ModificationDate = DateOnly.ParseExact(
				p[18],
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture)
		};
	}
}
