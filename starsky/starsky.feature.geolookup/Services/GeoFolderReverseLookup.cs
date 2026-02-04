using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.foundation.database.Models;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;

namespace starsky.feature.geolookup.Services;

/// <summary>
///     Internal API - Getting GeoData
/// </summary>
/// <param name="reverseLookup">reverse geocode</param>
/// <param name="memoryCache">for keeping status</param>
[Service(typeof(IGeoFolderReverseLookup), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class GeoFolderReverseLookup(
	IReverseGeoCodeService reverseLookup,
	IMemoryCache? memoryCache = null) : IGeoFolderReverseLookup
{
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
		if ( subPath == null )
		{
			return metaFilesInDirectory;
		}

		new GeoCacheStatusService(memoryCache).StatusUpdate(subPath, metaFilesInDirectory.Count * 2,
			StatusType.Total);

		foreach ( var metaFileItem in metaFilesInDirectory.Select((value, index) =>
			         new { value, index }) )
		{
			var result =
				await reverseLookup.GetLocation(metaFileItem.value.Latitude,
					metaFileItem.value.Longitude);
			new GeoCacheStatusService(memoryCache).StatusUpdate(metaFileItem.value.ParentDirectory!,
				metaFileItem.index, StatusType.Current);
			if ( !result.IsSuccess )
			{
				continue;
			}

			metaFileItem.value.LocationCity = result.LocationCity;
			metaFileItem.value.LocationState = result.LocationState;
			metaFileItem.value.LocationCountry = result.LocationCountry;
			metaFileItem.value.LocationCountryCode = result.LocationCountryCode;
		}

		// Ready signal
		new GeoCacheStatusService(memoryCache).StatusUpdate(subPath,
			metaFilesInDirectory.Count, StatusType.Current);

		return metaFilesInDirectory;
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
		{
			return metaFilesInDirectory.Where(metaFileItem =>
					Math.Abs(metaFileItem.Latitude) > 0.001 &&
					Math.Abs(metaFileItem.Longitude) > 0.001)
				.ToList();
		}

		// the default situation
		return metaFilesInDirectory.Where(metaFileItem =>
			Math.Abs(metaFileItem.Latitude) > 0.001 && Math.Abs(metaFileItem.Longitude) > 0.001
			                                        && ( string.IsNullOrEmpty(metaFileItem
				                                             .LocationCity)
			                                             || string.IsNullOrEmpty(metaFileItem
				                                             .LocationCountryCode)
			                                             // LocationState can be empty
			                                             || string.IsNullOrEmpty(metaFileItem
				                                             .LocationCountry)
			                                        )
			                                        && ExtensionRolesHelper
				                                        .IsExtensionExifToolSupported(
					                                        metaFileItem.FileName)
		).ToList();
	}
}
