using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.feature.checkForUpdates.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.VersionHelpers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.health.UpdateCheck
{
	// todo add decorator
	public class CheckForUpdatesHelper
	{
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly AppSettings _appSettings;
		
		internal const string GithubApi = "https://api.github.com/repos/qdraw/starsky/releases";
		
		public CheckForUpdatesHelper(IHttpClientHelper httpClientHelper, AppSettings appSettings)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
		}

		public async Task<bool?> IsUpdateNeeded()
		{
			return await IsUpdateNeeded(_appSettings.AppVersion);
		}

		public async Task<bool?> IsUpdateNeeded(string currentVersion)
		{
			if ( string.IsNullOrWhiteSpace(currentVersion) ) throw new ArgumentNullException(nameof(currentVersion));
			if ( !_appSettings.CheckForUpdates ) return null;
			var (key, value) = await _httpClientHelper.ReadString(GithubApi);
			if ( !key ) return null;
			
			var releaseModelList = JsonSerializer.Deserialize<List<ReleaseModel>>(value, new JsonSerializerOptions());
			
			var tagName = releaseModelList.LastOrDefault(p => !p.Draft && !p.PreRelease)?.TagName;
			if ( string.IsNullOrWhiteSpace(tagName) || !tagName.StartsWith("v") ) return null;

			var latestVersion = tagName.Remove(0, 1);
			
			return SemVersion.Parse(currentVersion) > SemVersion.Parse(latestVersion);
		}

	}
}
