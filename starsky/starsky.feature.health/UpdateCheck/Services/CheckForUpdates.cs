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
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.VersionHelpers;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.health.UpdateCheck.Services
{
	[Service(typeof(ICheckForUpdates), InjectionLifetime = InjectionLifetime.Singleton)]
	public class CheckForUpdates : ICheckForUpdates
	{
		internal const string GithubStarskyReleaseApi =
			"https://api.github.com/repos/qdraw/starsky/releases";

		internal const string GithubStarskyReleaseMirrorApi =
			"https://qdraw.nl/special/starsky/releases";

		internal const string QueryCheckForUpdatesCacheName = "CheckForUpdates";

		private readonly AppSettings? _appSettings;
		private readonly IMemoryCache? _cache;
		private readonly IHttpClientHelper _httpClientHelper;

		public CheckForUpdates(IHttpClientHelper httpClientHelper, AppSettings? appSettings,
			IMemoryCache? cache)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_cache = cache;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentVersion">defaults to _appSettings</param>
		/// <returns></returns>
		public async Task<(UpdateStatus, string?)> IsUpdateNeeded(
			string currentVersion = "")
		{
			if ( _appSettings == null || _appSettings.CheckForUpdates == false )
			{
				return (UpdateStatus.Disabled, string.Empty);
			}

			currentVersion = string.IsNullOrWhiteSpace(currentVersion)
				? _appSettings.AppVersion
				: currentVersion;

			// The CLI programs uses no cache
			if ( _cache == null || _appSettings.AddMemoryCache != true )
			{
				return Parse(await QueryIsUpdateNeededAsync(), currentVersion);
			}

			if ( _cache.TryGetValue(QueryCheckForUpdatesCacheName,
					out var cacheResult) && cacheResult != null )
			{
				return Parse(( List<ReleaseModel> ) cacheResult, currentVersion);
			}

			cacheResult = await QueryIsUpdateNeededAsync();

			_cache.Set(QueryCheckForUpdatesCacheName, cacheResult,
				new TimeSpan(48, 0, 0));

			return Parse(( List<ReleaseModel>? ) cacheResult, currentVersion);
		}


		internal async Task<List<ReleaseModel>?> QueryIsUpdateNeededAsync()
		{
			// argument check is done in QueryIsUpdateNeeded
			var (key, value) = await _httpClientHelper.ReadString(GithubStarskyReleaseApi);
			if ( !key )
			{
				(key, value) = await _httpClientHelper.ReadString(GithubStarskyReleaseMirrorApi);
			}

			return !key
				? new List<ReleaseModel>()
				: JsonSerializer.Deserialize<List<ReleaseModel>>(value,
					DefaultJsonSerializer.CamelCase);
		}

		/// <summary>
		/// Parse the result from the API
		/// </summary>
		/// <param name="releaseModelList">inputModel</param>
		/// <param name="currentVersion">The current Version</param>
		/// <returns>Status and LatestVersion</returns>
		internal static (UpdateStatus, string) Parse(IEnumerable<ReleaseModel>? releaseModelList,
			string currentVersion)
		{
			var orderedReleaseModelList =
				// remove v at start
				releaseModelList?.OrderByDescending(p => SemVersion.Parse(p.TagName, false));

			var tagName = orderedReleaseModelList?
				.FirstOrDefault(p => p is { Draft: false, PreRelease: false })?.TagName;

			if ( string.IsNullOrWhiteSpace(tagName) ||
				 !tagName.StartsWith('v') )
			{
				return (UpdateStatus.NoReleasesFound, string.Empty);
			}

			try
			{
				// remove v at start
				var latestVersion = SemVersion.Parse(tagName);
				var currentVersionObject = SemVersion.Parse(currentVersion);
				var isNewer = latestVersion > currentVersionObject;
				var status = isNewer
					? UpdateStatus.NeedToUpdate
					: UpdateStatus.CurrentVersionIsLatest;
				return (status, latestVersion.ToString());
			}
			catch ( ArgumentException )
			{
				return (UpdateStatus.InputNotValid, string.Empty);
			}
		}
	}
}
