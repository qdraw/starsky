using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using NGeoNames;
using NGeoNames.Entities;
using starsky.foundation.geo.GeoDownload;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.geo.ReverseGeoCode.Model;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Helpers;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.geo.ReverseGeoCode;

/// <summary>
///     It's a singleton because the geodata is stored in-memory
/// </summary>
[Service(typeof(IReverseGeoCodeService), InjectionLifetime = InjectionLifetime.Singleton)]
public class ReverseGeoCodeService : IReverseGeoCodeService
{
	private readonly AppSettings _appSettings;
	private readonly IGeoFileDownload _geoFileDownload;
	private readonly IWebLogger _logger;
	private IEnumerable<Admin1Code>? _admin1CodesAscii;
	private ReverseGeoCode<ExtendedGeoName>? _reverseGeoCode;

	/// <summary>
	///     Getting GeoData
	/// </summary>
	/// <param name="appSettings">to know where to store the deps files</param>
	/// <param name="serviceScopeFactory">used to get IGeoFileDownload - Abstraction to download Geo Data</param>
	/// <param name="logger">debug logger</param>
	public ReverseGeoCodeService(AppSettings appSettings,
		IServiceScopeFactory serviceScopeFactory, IWebLogger logger)
	{
		_appSettings = appSettings;
		_geoFileDownload = serviceScopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IGeoFileDownload>();
		_logger = logger;
	}

	internal ReverseGeoCodeService(AppSettings appSettings,
		IGeoFileDownload geoFileDownload, IWebLogger logger)
	{
		_appSettings = appSettings;
		_geoFileDownload = geoFileDownload;
		_logger = logger;
	}

	public async Task<GeoLocationModel> GetLocation(double latitude, double longitude)
	{
		if ( _reverseGeoCode == null )
		{
			( _, _reverseGeoCode ) = await SetupAsync();
		}

		var status = new GeoLocationModel
		{
			Longitude = longitude,
			Latitude = latitude,
			IsSuccess = false,
			ErrorReason = "Unknown"
		};

		if ( !ValidateLocation.ValidateLatitudeLongitude(latitude, longitude) )
		{
			status.ErrorReason = "Non-valid location";
			return status;
		}

		// Create a point from a lat/long pair from which we want to conduct our search(es) (center)
		var place = _reverseGeoCode.CreateFromLatLong(
			status.Latitude, status.Longitude);

		// Find nearest
		var nearestPlace = _reverseGeoCode.NearestNeighbourSearch(place, 1).FirstOrDefault();

		if ( nearestPlace == null )
		{
			status.ErrorReason = "No nearest place found";
			return status;
		}

		// Distance to avoid non logic locations
		var distanceTo = GeoDistanceTo.GetDistance(
			nearestPlace.Latitude,
			nearestPlace.Longitude,
			status.Latitude,
			status.Longitude);

		if ( distanceTo > 35 )
		{
			status.ErrorReason = "Distance to nearest place is too far";
			return status;
		}

		status.ErrorReason = "Success";
		status.IsSuccess = true;
		status.LocationCity = nearestPlace.NameASCII;

		// Catch is used for example the region VA (Vatican City)
		try
		{
			var region = new RegionInfo(nearestPlace.CountryCode);
			status.LocationCountry = region.EnglishName;
			status.LocationCountryCode = region.ThreeLetterISORegionName;
		}
		catch ( ArgumentException e )
		{
			_logger.LogInformation("[GeoReverseLookup] " + e.Message);
		}

		status.LocationState = GetAdmin1Name(nearestPlace.CountryCode, nearestPlace.Admincodes);

		return status;
	}

	internal string? GetAdmin1Name(string countryCode, string[] adminCodes)
	{
		if ( _admin1CodesAscii == null || adminCodes.Length != 4 )
		{
			return null;
		}

		var admin1Code = countryCode + "." + adminCodes[0];

		var admin2Object = _admin1CodesAscii.FirstOrDefault(p => p.Code == admin1Code);
		return admin2Object?.NameASCII;
	}

	internal async Task<(IEnumerable<Admin1Code>, ReverseGeoCode<ExtendedGeoName>)> SetupAsync()
	{
		await _geoFileDownload.DownloadAsync();

		_admin1CodesAscii = GeoFileReader.ReadAdmin1Codes(
			Path.Combine(_appSettings.DependenciesFolder, "admin1CodesASCII.txt"));

		_reverseGeoCode = new ReverseGeoCode<ExtendedGeoName>(
			GeoFileReader.ReadExtendedGeoNames(
				Path.Combine(_appSettings.DependenciesFolder,
					GeoFileDownload.CountryName + ".txt")));

		return ( _admin1CodesAscii, _reverseGeoCode );
	}
}
