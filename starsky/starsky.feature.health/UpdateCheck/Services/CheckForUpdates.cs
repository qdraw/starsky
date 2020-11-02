using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.health.UpdateCheck.Interfaces;
using starsky.feature.health.UpdateCheck.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.VersionHelpers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.health.UpdateCheck.Services
{
	[Service(typeof(ICheckForUpdates), InjectionLifetime = InjectionLifetime.Singleton)]
	public class CheckForUpdates : ICheckForUpdates
	{
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly AppSettings _appSettings;
		private readonly IMemoryCache _cache;

		internal const string GithubApi = "https://api.github.com/repos/qdraw/starsky/releases";
		
		public CheckForUpdates(IHttpClientHelper httpClientHelper, AppSettings appSettings, IMemoryCache cache)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_cache = cache;
		}
		
		public async Task<UpdateStatus> IsUpdateNeeded()
		{
			if ( _appSettings == null ) return UpdateStatus.Disabled;
			// The CLI programs uses no cache
			if( _cache == null || _appSettings?.AddMemoryCache == false) 
				return await QueryIsUpdateNeeded(_appSettings.AppVersion);
            
			// Return values from IMemoryCache
			var queryCacheName = "CheckForUpdates";

			if (_cache.TryGetValue(queryCacheName, out var cacheResult))
				return (UpdateStatus) cacheResult;

			cacheResult = await QueryIsUpdateNeeded(_appSettings.AppVersion);

			// Set only when query has been an success
			if ( (UpdateStatus) cacheResult != UpdateStatus.HttpError  ) {
				
				_cache.Set(queryCacheName, cacheResult, 
				new TimeSpan(48,0,0));
			}
			
			return (UpdateStatus) cacheResult;
		}

		public async Task<UpdateStatus> QueryIsUpdateNeeded(string currentVersion)
		{
			if ( string.IsNullOrWhiteSpace(currentVersion) ) throw new ArgumentNullException(nameof(currentVersion));
			if ( !_appSettings.CheckForUpdates ) return UpdateStatus.Disabled;
			var (key, value) = await _httpClientHelper.ReadString(GithubApi);
			if ( !key ) return UpdateStatus.HttpError;
			
			var releaseModelList = JsonSerializer.Deserialize<List<ReleaseModel>>(value, new JsonSerializerOptions());
			
			var tagName = releaseModelList.LastOrDefault(p => !p.Draft && !p.PreRelease)?.TagName;
			if ( string.IsNullOrWhiteSpace(tagName) || !tagName.StartsWith("v") ) return UpdateStatus.NoReleasesFound;

			var latestVersion = tagName.Remove(0, 1);
			
			return SemVersion.Parse(currentVersion) >= SemVersion.Parse(latestVersion) ? UpdateStatus.CurrentVersionIsLatest : UpdateStatus.NeedToUpdate;
		}

	}
}
