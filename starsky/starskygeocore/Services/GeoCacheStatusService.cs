using System;
using Microsoft.Extensions.Caching.Memory;
using starskygeocore.Models;

namespace starskygeocore.Services
{
	public class GeoCacheStatusService
	{
		private readonly IMemoryCache _cache;
		
		public GeoCacheStatusService( IMemoryCache memoryCache = null)
		{
			_cache = memoryCache;
		}


		public GeoCacheStatus Status(string path)
		{
			if(_cache == null || string.IsNullOrWhiteSpace(path)) return new GeoCacheStatus{Total = -1};

			var totalCacheName = nameof(GeoCacheStatus) + path + StatusType.Total;
			var result = new GeoCacheStatus();
			
			if(_cache.TryGetValue(totalCacheName, out var statusObjectTotal))
			{
				int.TryParse(statusObjectTotal.ToString(), out var status);
				result.Total = status;
			}
			
			var currentCacheName = nameof(GeoCacheStatus) + path + StatusType.Current;
			if(_cache.TryGetValue(currentCacheName, out var statusObjectCurrent))
			{
				int.TryParse(statusObjectCurrent.ToString(), out var status);
				result.Current = status;
			}
			
			return result;
		}
        
		public void Update(string path, int current, StatusType type)
		{
			if(_cache == null || string.IsNullOrWhiteSpace(path)) return;
			
			var queryCacheName = nameof(GeoCacheStatus) + path + type;
			_cache.Set(queryCacheName, current, new TimeSpan(10,0,0));
		}

	}
}
