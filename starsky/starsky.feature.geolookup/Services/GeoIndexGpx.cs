using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.storage.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.geolookup.Services
{
	public class GeoIndexGpx : IGeoIndexGpx
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IMemoryCache? _cache;
		private readonly IWebLogger _logger;

		public GeoIndexGpx(AppSettings appSettings, IStorage iStorage,
			IWebLogger logger, IMemoryCache? memoryCache = null)
		{
			_appSettings = appSettings;
			_iStorage = iStorage;
			_cache = memoryCache;
			_logger = logger;
		}

		private static List<FileIndexItem> GetNoLocationItems(IEnumerable<FileIndexItem> metaFilesInDirectory)
		{
			return metaFilesInDirectory.Where(
					metaFileItem =>
						( Math.Abs(metaFileItem.Latitude) < 0.001 && Math.Abs(metaFileItem.Longitude) < 0.001 )
						&& metaFileItem.DateTime.Year > 2) // ignore files without a date
				.ToList();
		}

		private async Task<List<GeoListItem>> GetGpxFileAsync(List<FileIndexItem> metaFilesInDirectory)
		{
			var geoList = new List<GeoListItem>();
			foreach ( var metaFileItem in metaFilesInDirectory )
			{

				if ( !ExtensionRolesHelper.IsExtensionForceGpx(metaFileItem.FileName) )
				{
					continue;
				}

				if ( !_iStorage.ExistFile(metaFileItem.FilePath!) )
				{
					continue;
				}

				using ( var stream = _iStorage.ReadStream(metaFileItem.FilePath!) )
				{
					geoList.AddRange(await new ReadMetaGpx(_logger).ReadGpxFileAsync(stream, geoList));
				}
			}
			return geoList;
		}

		/// <summary>
		/// Convert to the appSettings timezone setting
		/// </summary>
		/// <param name="valueDateTime">current DateTime</param>
		/// <param name="subPath">optional only to display errors</param>
		/// <returns>The time in the specified timezone</returns>
		/// <exception cref="ArgumentException">DateTime Kind should not be Local</exception>
		internal DateTime ConvertTimeZone(DateTime valueDateTime, string subPath = "")
		{
			if ( valueDateTime.Kind == DateTimeKind.Utc )
			{
				return valueDateTime;
			}

			// Not supported by TimeZoneInfo convert
			if ( valueDateTime.Kind != DateTimeKind.Unspecified || _appSettings.CameraTimeZoneInfo == null )
			{
				throw new ArgumentException($"valueDateTime DateTime-Kind '{valueDateTime.Kind}' " +
											$"'{subPath}' should be Unspecified", nameof(valueDateTime));
			}

			return TimeZoneInfo.ConvertTime(valueDateTime,
				_appSettings.CameraTimeZoneInfo, TimeZoneInfo.Utc);
		}

		public async Task<List<FileIndexItem>> LoopFolderAsync(List<FileIndexItem> metaFilesInDirectory)
		{
			var toUpdateMetaFiles = new List<FileIndexItem>();

			var gpxList = await GetGpxFileAsync(metaFilesInDirectory);
			if ( gpxList.Count == 0 )
			{
				return toUpdateMetaFiles;
			}

			metaFilesInDirectory = GetNoLocationItems(metaFilesInDirectory);

			var subPath = metaFilesInDirectory.FirstOrDefault()?.ParentDirectory!;
			new GeoCacheStatusService(_cache).StatusUpdate(subPath,
				metaFilesInDirectory.Count, StatusType.Total);

			foreach ( var metaFileItem in metaFilesInDirectory.Select(
						 (value, index) => new { value, index }) )
			{
				var dateTimeCameraUtc = ConvertTimeZone(metaFileItem.value.DateTime,
					metaFileItem.value.FilePath!);

				var fileGeoData = gpxList.MinBy(p => Math.Abs(( p.DateTime - dateTimeCameraUtc ).Ticks));
				if ( fileGeoData == null )
				{
					continue;
				}

				var minutesDifference = ( dateTimeCameraUtc - fileGeoData.DateTime ).TotalMinutes;
				if ( minutesDifference < -5 || minutesDifference > 5 )
				{
					continue;
				}

				metaFileItem.value.Latitude = fileGeoData.Latitude;
				metaFileItem.value.Longitude = fileGeoData.Longitude;
				metaFileItem.value.LocationAltitude = fileGeoData.Altitude;

				toUpdateMetaFiles.Add(metaFileItem.value);

				// status update
				new GeoCacheStatusService(_cache).StatusUpdate(metaFileItem.value.ParentDirectory!,
					metaFileItem.index, StatusType.Current);
			}

			// Ready signal
			new GeoCacheStatusService(_cache).StatusUpdate(subPath,
				metaFilesInDirectory.Count, StatusType.Current);

			return toUpdateMetaFiles;
		}
	}
}
