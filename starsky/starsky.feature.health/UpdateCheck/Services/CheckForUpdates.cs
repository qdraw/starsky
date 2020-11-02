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
		internal const string GithubApi = "https://api.github.com/repos/qdraw/starsky/releases";
		private readonly AppSettings _appSettings;
		private readonly IMemoryCache _cache;
		private readonly IHttpClientHelper _httpClientHelper;

		public CheckForUpdates(IHttpClientHelper httpClientHelper, AppSettings appSettings, IMemoryCache cache)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_cache = cache;
		}

		public async Task<KeyValuePair<UpdateStatus, string>> IsUpdateNeeded()
		{
        	if ( !_appSettings.CheckForUpdates ) return new KeyValuePair<UpdateStatus, string>(UpdateStatus.Disabled,"");
        			
			if ( _appSettings == null ) 
				return new KeyValuePair<UpdateStatus, string>(UpdateStatus.Disabled, string.Empty);
			
			// The CLI programs uses no cache
			if( _cache == null || _appSettings?.AddMemoryCache == false) 
				return await QueryIsUpdateNeeded(_appSettings.AppVersion);
            
			// Return values from IMemoryCache
			const string queryCacheName = "CheckForUpdates";

			if (_cache.TryGetValue(queryCacheName, out var cacheResult))
				return (KeyValuePair<UpdateStatus, string>) cacheResult;

			cacheResult = await QueryIsUpdateNeeded(_appSettings.AppVersion);

			// Set only when query has been an success
			var status = (( KeyValuePair<UpdateStatus, string> ) cacheResult).Key;
			if ( status != UpdateStatus.HttpError  ) {
				
				_cache.Set(queryCacheName, cacheResult, 
				new TimeSpan(48,0,0));
			}
			
			return ( KeyValuePair<UpdateStatus, string> ) cacheResult;
		}

		public Task<KeyValuePair<UpdateStatus, string>> QueryIsUpdateNeeded(string currentVersion)
		{
			if ( string.IsNullOrWhiteSpace(currentVersion) ) throw new ArgumentNullException(nameof(currentVersion));
		    return QueryIsUpdateNeededAsync(currentVersion);
		}

		private async Task<KeyValuePair<UpdateStatus, string>> QueryIsUpdateNeededAsync(string currentVersion)
		{
			// argument check is done in QueryIsUpdateNeeded
			var (key, value) = await _httpClientHelper.ReadString(GithubApi);
			if ( !key ) return new KeyValuePair<UpdateStatus, string>(UpdateStatus.HttpError,value);
			
			var releaseModelList = JsonSerializer.Deserialize<List<ReleaseModel>>(value, new JsonSerializerOptions());
			
			var tagName = releaseModelList.LastOrDefault(p => !p.Draft && !p.PreRelease)?.TagName;
			if ( string.IsNullOrWhiteSpace(tagName) || !tagName.StartsWith("v") )
				return new KeyValuePair<UpdateStatus, string>(UpdateStatus.NoReleasesFound,value);

			var latestVersion = tagName.Remove(0, 1);
			
			var status =  SemVersion.Parse(currentVersion) >= 
			             SemVersion.Parse(latestVersion) ? UpdateStatus.CurrentVersionIsLatest : UpdateStatus.NeedToUpdate;
			return new KeyValuePair<UpdateStatus, string>(status, latestVersion);
		}
	}
}
