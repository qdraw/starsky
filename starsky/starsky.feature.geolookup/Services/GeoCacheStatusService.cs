using System;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.geolookup.Models;
using static System.Int32;

namespace starsky.feature.geolookup.Services
{
	public class GeoCacheStatusService
	{
		private readonly IMemoryCache? _cache;
		
		public GeoCacheStatusService( IMemoryCache? memoryCache = null)
		{
			_cache = memoryCache;
		}

		public GeoCacheStatus Status(string path)
		{
			if(_cache == null || string.IsNullOrWhiteSpace(path)) return new GeoCacheStatus{Total = -1};

			var totalCacheName = nameof(GeoCacheStatus) + path + StatusType.Total;
			var result = new GeoCacheStatus();
			
			if(_cache.TryGetValue(totalCacheName, out var statusObjectTotal) && 
			   TryParse(statusObjectTotal.ToString(), out var totalStatus))
			{
				result.Total = totalStatus;
			}
			
			var currentCacheName = nameof(GeoCacheStatus) + path + StatusType.Current;
			if(_cache.TryGetValue(currentCacheName, out var statusObjectCurrent) && 
			   TryParse(statusObjectCurrent.ToString(), out var currentStatus))
			{
				result.Current = currentStatus;
			}
			
			return result;
		}
        
		public void StatusUpdate(string path, int current, StatusType type)
		{
			if(_cache == null || string.IsNullOrWhiteSpace(path)) return;
			
			var queryGeoCacheName = nameof(GeoCacheStatus) + path + type;
			_cache.Set(queryGeoCacheName, current, new TimeSpan(10,0,0));
		}

	}
}
