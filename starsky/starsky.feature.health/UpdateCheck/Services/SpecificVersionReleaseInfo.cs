using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.health.UpdateCheck.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.health.UpdateCheck.Services;

[Service(typeof(ISpecificVersionReleaseInfo), InjectionLifetime = InjectionLifetime.Singleton)]
public class SpecificVersionReleaseInfo : ISpecificVersionReleaseInfo
{
	internal const string GetSpecificVersionReleaseInfoCacheName = "GetSpecificVersionReleaseInfo";

	internal const string SpecificVersionReleaseInfoUrl = "qdraw.nl/special/starsky/releaseinfo";

	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IMemoryCache? _cache;
	private readonly IWebLogger _webLogger;
	private readonly AppSettings? _appSettings;

	public SpecificVersionReleaseInfo(IHttpClientHelper httpClientHelper, AppSettings? appSettings,
		IMemoryCache? cache, IWebLogger webLogger)
	{
		_appSettings = appSettings;
		_httpClientHelper = httpClientHelper;
		_cache = cache;
		_webLogger = webLogger;
	}

	public async Task<string> SpecificVersionMessage(string? versionToCheckFor)
	{
		if ( _cache == null || _appSettings?.AddMemoryCache != true )
		{
			var specificVersionReleaseInfoContent =
				await QuerySpecificVersionInfo();
			return Parse(specificVersionReleaseInfoContent, versionToCheckFor);
		}

		if ( _cache.TryGetValue(GetSpecificVersionReleaseInfoCacheName,
			    out var cacheResult) && cacheResult != null )
		{
			return Parse(( string )cacheResult, versionToCheckFor);
		}

		cacheResult = await QuerySpecificVersionInfo();
		_cache.Set(GetSpecificVersionReleaseInfoCacheName, cacheResult,
			new TimeSpan(48, 0, 0));

		return Parse(( string )cacheResult, versionToCheckFor);
	}

	internal string Parse(string json, string? latestVersion)
	{
		if ( string.IsNullOrWhiteSpace(json) )
		{
			return string.Empty;
		}

		latestVersion ??= string.Empty;
		var dict = new Dictionary<string, Dictionary<string, string>>();
		try
		{
			dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
		}
		catch ( JsonException e )
		{
			_webLogger.LogError("[SpecificVersionReleaseInfo] Json parse error: " + e.Message);
		}

		if ( dict?.TryGetValue(latestVersion, out var valueDict) is not true ) return string.Empty;

		var outputValue = valueDict.TryGetValue("en", out var languageValue)
			? ConvertMarkdownLinkToHtml(languageValue)
			: string.Empty;

		return outputValue;
	}

	internal static string ConvertMarkdownLinkToHtml(string markdown)
	{
		// Regular expression to match Markdown links
		const string pattern = @"\[(.*?)\]\((.*?)\)";

		// Replace Markdown links with HTML anchor tags
		return Regex.Replace(markdown, pattern,
			"<a href=\"$2\">$1</a>", RegexOptions.None,
			TimeSpan.FromMilliseconds(100));
	}

	internal async Task<string> QuerySpecificVersionInfo()
	{
		// argument check is done in QueryIsUpdateNeeded
		var (key, value) = await _httpClientHelper.ReadString(
			"https://" + SpecificVersionReleaseInfoUrl);

		if ( key ) return value;
		_webLogger.LogInformation($"[SpecificVersionReleaseInfo] {value} [end]");
		return string.Empty;
	}
}
