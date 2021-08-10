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
		
		internal const string QueryCheckForUpdatesCacheName = "CheckForUpdates";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentVersion">defaults to _appSettings</param>
		/// <returns></returns>
		public async Task<KeyValuePair<UpdateStatus, string>> IsUpdateNeeded(string currentVersion = "")
		{
			if (_appSettings == null || _appSettings.CheckForUpdates == false ) 
				return new KeyValuePair<UpdateStatus, string>(UpdateStatus.Disabled,"");

			currentVersion = string.IsNullOrWhiteSpace(currentVersion)
				?  _appSettings.AppVersion : currentVersion;
			
			// The CLI programs uses no cache
			if( _cache == null || _appSettings?.AddMemoryCache == false) 
				return Parse(await QueryIsUpdateNeededAsync(),currentVersion);

			if ( _cache.TryGetValue(QueryCheckForUpdatesCacheName, out var cacheResult) )
				return Parse(( List<ReleaseModel> ) cacheResult, currentVersion);

			cacheResult = await QueryIsUpdateNeededAsync();

			_cache.Set(QueryCheckForUpdatesCacheName, cacheResult, 
			new TimeSpan(48,0,0));

			return Parse(( List<ReleaseModel> ) cacheResult,currentVersion);
		}

		internal async Task<List<ReleaseModel>> QueryIsUpdateNeededAsync()
		{
			// argument check is done in QueryIsUpdateNeeded
			var (key, value) = await _httpClientHelper.ReadString(GithubApi);
			return !key ? new List<ReleaseModel>() : JsonSerializer.Deserialize<List<ReleaseModel>>(value, new JsonSerializerOptions());
		}

		// ReSharper disable once MemberCanBeMadeStatic.Global
		internal KeyValuePair<UpdateStatus, string> Parse(IEnumerable<ReleaseModel> releaseModelList, string currentVersion )
		{
			var orderedReleaseModelList = releaseModelList.OrderByDescending(p => p.TagName);
			var tagName = orderedReleaseModelList.FirstOrDefault(p => !p.Draft && !p.PreRelease)?.TagName;
			if ( string.IsNullOrWhiteSpace(tagName) || !tagName.StartsWith("v") )
				return new KeyValuePair<UpdateStatus, string>(UpdateStatus.NoReleasesFound,string.Empty);

			try
			{
				var latestVersion = SemVersion.Parse(tagName.Remove(0, 1));
				var currentVersionObject = SemVersion.Parse(currentVersion);
				var isNewer = latestVersion > currentVersionObject;
				var status = isNewer ? UpdateStatus.NeedToUpdate : UpdateStatus.CurrentVersionIsLatest;
				return new KeyValuePair<UpdateStatus, string>(status, latestVersion.ToString());
			}
			catch ( ArgumentException)
			{
				return new KeyValuePair<UpdateStatus, string>(UpdateStatus.InputNotValid, string.Empty);
			}

		}
	}
}
