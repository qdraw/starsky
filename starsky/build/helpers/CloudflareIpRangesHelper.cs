using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Serilog;

namespace helpers;

/// <summary>
///     Checks Cloudflare's published IP ranges (via their public API) against the hardcoded list
///     in starsky.foundation.platform/Helpers/CloudflareTrustedProxiesHelper.cs, and rewrites
///     that file when they've changed. Never fails the build: any network/parse problem (e.g. no
///     internet) is logged as a warning and skipped.
/// </summary>
public static class CloudflareIpRangesHelper
{
#pragma warning disable S1075 // the public Cloudflare API endpoint is intentionally fixed
	const string ApiUrl = "https://api.cloudflare.com/client/v4/ips";
#pragma warning restore S1075

	static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

	const string TargetRelativePath =
		"starsky.foundation.platform/Helpers/CloudflareTrustedProxiesHelper.cs";

	const string BeginMarker = "// BEGIN-CLOUDFLARE-IP-RANGES";
	const string EndMarker = "// END-CLOUDFLARE-IP-RANGES";

	public static void CheckAndUpdate(string solutionParentFolder)
	{
		var filePath = Path.Combine(solutionParentFolder,
			TargetRelativePath.Replace('/', Path.DirectorySeparatorChar));

		if ( !File.Exists(filePath) )
		{
			Log.Warning(
				"CheckCloudflareIpRanges: file not found, skipping: {FilePath}", filePath);
			return;
		}

		List<string>? freshRanges;
		try
		{
			var json = HttpQuery.GetJsonFromApi(ApiUrl).GetAwaiter().GetResult();
			if ( string.IsNullOrEmpty(json) )
			{
				Log.Warning(
					"CheckCloudflareIpRanges: no response from {ApiUrl} " +
					"(no internet?), skipping", ApiUrl);
				return;
			}

			freshRanges = ParseIpRanges(json);
		}
		catch ( Exception exception )
		{
			// Any failure here (DNS, timeout, bad json, ...) should never break the build
			Log.Warning(exception,
				"CheckCloudflareIpRanges: failed to fetch/parse Cloudflare's IP ranges " +
				"(no internet?), skipping");
			return;
		}

		if ( freshRanges == null || freshRanges.Count == 0 )
		{
			Log.Warning(
				"CheckCloudflareIpRanges: API response had no ranges, skipping");
			return;
		}

		var content = File.ReadAllText(filePath);
		var currentRanges = ExtractCurrentRanges(content);

		if ( currentRanges != null &&
			 new HashSet<string>(currentRanges).SetEquals(freshRanges) )
		{
			Log.Information(
				"CheckCloudflareIpRanges: list is up to date ({Count} ranges)",
				freshRanges.Count);
			return;
		}

		if ( currentRanges == null )
		{
			Log.Warning(
				"CheckCloudflareIpRanges: could not find the markers in {FilePath}, skipping",
				filePath);
			return;
		}

		File.WriteAllText(filePath, ReplaceRanges(content, freshRanges));

		Log.Information(
			"CheckCloudflareIpRanges: {FilePath} updated with {Count} ranges " +
			"— please review and commit", filePath, freshRanges.Count);
	}

	static List<string> ParseIpRanges(string json)
	{
		using var document = JsonDocument.Parse(json);
		var result = document.RootElement.GetProperty("result");

		var ranges = new List<string>();
		foreach ( var propertyName in new[] { "ipv4_cidrs", "ipv6_cidrs" } )
		{
			if ( !result.TryGetProperty(propertyName, out var arrayElement) )
			{
				continue;
			}

			ranges.AddRange(arrayElement.EnumerateArray()
				.Select(element => element.GetString())
				.Where(value => !string.IsNullOrWhiteSpace(value))!);
		}

		return ranges;
	}

	static List<string>? ExtractCurrentRanges(string content)
	{
		var match = Regex.Match(content,
			Regex.Escape(BeginMarker) + "(.*?)" + Regex.Escape(EndMarker),
			RegexOptions.Singleline, RegexTimeout);

		if ( !match.Success )
		{
			return null;
		}

		return Regex.Matches(match.Groups[1].Value, "\"([^\"]+)\"",
				RegexOptions.None, RegexTimeout)
			.Select(m => m.Groups[1].Value)
			.ToList();
	}

	static string ReplaceRanges(string content, List<string> ranges)
	{
		var body = string.Join("\n",
			ranges.Select(range => $"\t\t\"{range}\","));
		// drop the trailing comma on the last entry to match the existing style
		body = body.TrimEnd(',');

		var replacement = $"{BeginMarker}\n{body}\n\t\t{EndMarker}";

		var updated = Regex.Replace(content,
			Regex.Escape(BeginMarker) + ".*?" + Regex.Escape(EndMarker),
			replacement.Replace("$", "$$"), RegexOptions.Singleline, RegexTimeout);

		var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
		updated = Regex.Replace(updated,
			@"LastChecked: \d{4}-\d{2}-\d{2}", $"LastChecked: {today}",
			RegexOptions.None, RegexTimeout);
		updated = Regex.Replace(updated,
			@"Last checked: \d{4}-\d{2}-\d{2}\.", $"Last checked: {today}.",
			RegexOptions.None, RegexTimeout);

		return updated;
	}
}
