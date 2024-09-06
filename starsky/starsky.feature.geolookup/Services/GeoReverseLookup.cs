using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NGeoNames;
using NGeoNames.Entities;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Helpers;

namespace starsky.feature.geolookup.Services;

[Service(typeof(IGeoReverseLookup), InjectionLifetime = InjectionLifetime.Singleton)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public sealed class GeoReverseLookup : IGeoReverseLookup
{
	private readonly AppSettings _appSettings;
	private readonly IMemoryCache? _cache;
	private readonly IGeoFileDownload _geoFileDownload;
	private readonly IWebLogger _logger;
	private IEnumerable<Admin1Code>? _admin1CodesAscii;
	private ReverseGeoCode<ExtendedGeoName>? _reverseGeoCode;

	/// <summary>
	///     Getting GeoData
	/// </summary>
	/// <param name="appSettings">to know where to store the deps files</param>
	/// <param name="serviceScopeFactory">used to get IGeoFileDownload - Abstraction to download Geo Data</param>
	/// <param name="memoryCache">for keeping status</param>
	/// <param name="logger">debug logger</param>
	public GeoReverseLookup(AppSettings appSettings,
		IServiceScopeFactory serviceScopeFactory, IWebLogger logger,
		IMemoryCache? memoryCache = null)
	{
		_appSettings = appSettings;
		_logger = logger;
		_geoFileDownload = serviceScopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IGeoFileDownload>();
		_reverseGeoCode = null;
		_admin1CodesAscii = null;
		_cache = memoryCache;
	}

	/// <summary>
	///     Internal API - Getting GeoData
	/// </summary>
	/// <param name="appSettings">to know where to store the deps files</param>
	/// <param name="geoFileDownload">Abstraction to download Geo Data</param>
	/// <param name="memoryCache">for keeping status</param>
	/// <param name="logger">debug logger</param>
	internal GeoReverseLookup(AppSettings appSettings, IGeoFileDownload geoFileDownload,
		IWebLogger logger, IMemoryCache? memoryCache = null)
	{
		_appSettings = appSettings;
		_logger = logger;
		// Get the IGeoFileDownload from the service scope due different injection lifetime (singleton vs scoped)
		_geoFileDownload = geoFileDownload;
		_reverseGeoCode = null;
		_admin1CodesAscii = null;
		_cache = memoryCache;
	}

	/// <summary>
	///     Reverse Geo Syncing for a folder
	/// </summary>
	/// <param name="metaFilesInDirectory">list of files to lookup</param>
	/// <param name="overwriteLocationNames">true = overwrite the location names, that have a gps location</param>
	/// <returns></returns>
	public async Task<List<FileIndexItem>> LoopFolderLookup(
		List<FileIndexItem> metaFilesInDirectory,
		bool overwriteLocationNames)
	{
		metaFilesInDirectory = RemoveNoUpdateItems(metaFilesInDirectory, overwriteLocationNames);

		var subPath = metaFilesInDirectory.FirstOrDefault()?.ParentDirectory;
		if ( subPath == null ) return metaFilesInDirectory;

		new GeoCacheStatusService(_cache).StatusUpdate(subPath, metaFilesInDirectory.Count * 2,
			StatusType.Total);

		foreach ( var metaFileItem in metaFilesInDirectory.Select(
			         (value, index) => new { value, index }) )
		{
			var result =
				await GetLocation(metaFileItem.value.Latitude, metaFileItem.value.Longitude);
			new GeoCacheStatusService(_cache).StatusUpdate(metaFileItem.value.ParentDirectory!,
				metaFileItem.index, StatusType.Current);
			if ( !result.IsSuccess ) continue;
			metaFileItem.value.LocationCity = result.LocationCity;
			metaFileItem.value.LocationState = result.LocationState;
			metaFileItem.value.LocationCountry = result.LocationCountry;
			metaFileItem.value.LocationCountryCode = result.LocationCountryCode;
		}

		// Ready signal
		new GeoCacheStatusService(_cache).StatusUpdate(subPath,
			metaFilesInDirectory.Count, StatusType.Current);

		return metaFilesInDirectory;
	}

	public async Task<GeoLocationModel> GetLocation(double latitude, double longitude)
	{
		if ( _reverseGeoCode == null ) ( _, _reverseGeoCode ) = await SetupAsync();

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
			status.LocationCountry = region.NativeName;
			status.LocationCountryCode = region.ThreeLetterISORegionName;
		}
		catch ( ArgumentException e )
		{
			_logger.LogInformation("[GeoReverseLookup] " + e.Message);
		}

		status.LocationState = GetAdmin1Name(nearestPlace.CountryCode, nearestPlace.Admincodes);

		return status;
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

	internal string? GetAdmin1Name(string countryCode, string[] adminCodes)
	{
		if ( _admin1CodesAscii == null || adminCodes.Length != 4 ) return null;

		var admin1Code = countryCode + "." + adminCodes[0];

		var admin2Object = _admin1CodesAscii.FirstOrDefault(p => p.Code == admin1Code);
		return admin2Object?.NameASCII;
	}

	/// <summary>
	///     Checks for files that already done
	///     if latitude is not location 0,0, That's default
	///     If one of the meta items are missing, keep in list
	///     If extension in exifTool supported, so no gpx
	/// </summary>
	/// <param name="metaFilesInDirectory">List of files with metadata</param>
	/// <param name="overwriteLocationNames">true = overwrite the location names, that have a gps location </param>
	/// <returns>list that can be updated</returns>
	public static List<FileIndexItem> RemoveNoUpdateItems(
		IEnumerable<FileIndexItem> metaFilesInDirectory,
		bool overwriteLocationNames)
	{
		// this will overwrite the location names, that have a gps location 
		if ( overwriteLocationNames )
			return metaFilesInDirectory.Where(
					metaFileItem =>
						Math.Abs(metaFileItem.Latitude) > 0.001 &&
						Math.Abs(metaFileItem.Longitude) > 0.001)
				.ToList();

		// the default situation
		return metaFilesInDirectory.Where(
			metaFileItem =>
				Math.Abs(metaFileItem.Latitude) > 0.001 && Math.Abs(metaFileItem.Longitude) > 0.001
				                                        && ( string.IsNullOrEmpty(metaFileItem
					                                             .LocationCity)
				                                             || string.IsNullOrEmpty(metaFileItem
					                                             .LocationState)
				                                             || string.IsNullOrEmpty(metaFileItem
					                                             .LocationCountry)
				                                        ) // for now NO check on: metaFileItem.LocationCountryCode
				                                        && ExtensionRolesHelper
					                                        .IsExtensionExifToolSupported(
						                                        metaFileItem.FileName)
		).ToList();
	}
}
