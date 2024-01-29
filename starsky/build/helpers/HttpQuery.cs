using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Serilog;


namespace helpers;

public static class HttpQuery
{

	[ItemCanBeNull]
	public static async Task<string> GetJsonFromApi(string apiUrl)
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
		catch ( HttpRequestException exception)
		{
			Log.Information($"GetJsonFromApi {exception.StatusCode} {exception.Message}");
			return null;
		}
	}
	
	public static Version ParseJsonVersionNumbers(string json)
	{
		var jsonDocument = JsonDocument.Parse(json);
		var rootElement = jsonDocument.RootElement;

		var versionsResult = new List<string>();
		if (rootElement.TryGetProperty("versions", out var versionsElement))
		{
			versionsResult = versionsElement.EnumerateArray().Select(v => v.GetString()).ToList();
		}

		var hightestVersion = versionsResult.MaxBy(v => new Version(v));
		return new Version(hightestVersion);
	}
}


