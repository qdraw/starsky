using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace helpers;

public static class HttpQuery
{
	public static async Task<string?> GetJsonFromApi(string apiUrl)
	{
		try
		{
			using var httpClient = new HttpClient();
			// Make a GET request to the API
			var response = await httpClient.GetAsync(apiUrl);

			// Check if the request was successful
			response.EnsureSuccessStatusCode();

			// Read the content as a string
			return await response.Content.ReadAsStringAsync();
		}
		catch ( HttpRequestException exception )
		{
			Log.Information("GetJsonFromApi {StatusCode} {Message}", exception.StatusCode,
				exception.Message);
			return null;
		}
	}

	public static Version ParseJsonVersionNumbers(string json)
	{
		var jsonDocument = JsonDocument.Parse(json);
		var rootElement = jsonDocument.RootElement;

		var versionsResult = new List<string>();
		if ( rootElement.TryGetProperty("versions", out var versionsElement) )
		{
			versionsResult = versionsElement.EnumerateArray().Select(v => v.GetString())
				.Cast<string>().ToList();
		}

		var hightestVersion = versionsResult.MaxBy(SafeVersion);
		return SafeVersion(hightestVersion);
	}

	private static Version SafeVersion(string? version)
	{
		if ( string.IsNullOrEmpty(version) )
		{
			return new Version(0,0);
		}
		try
		{
			return new Version(version);
		}
		catch ( FormatException )
		{
			return new Version(0,0);
		}
	}
}
