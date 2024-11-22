using System.Text.Json;

namespace starsky.foundation.geoimport;

public class Strava2Gpx
{
	public Strava2Gpx(string clientId, string clientSecret, string refreshToken)
	{
		ClientId = clientId;
		ClientSecret = clientSecret;
		RefreshToken = refreshToken;
		AccessToken = null;
		ActivitiesList = null;
		Streams = new Dictionary<string, int>
		{
			{ "latlng", 1 },
			{ "altitude", 1 },
			{ "heartrate", 0 },
			{ "cadence", 0 },
			{ "watts", 0 },
			{ "temp", 0 }
		};
	}

	private string ClientId { get; }
	private string ClientSecret { get; }
	private string RefreshToken { get; }
	private string AccessToken { get; set; }
	private List<List<object>> ActivitiesList { get; set; }
	private Dictionary<string, int> Streams { get; set; }

	public async Task ConnectAsync()
	{
		AccessToken = await RefreshAccessTokenAsync();
	}

	public async Task<List<List<object>>> GetActivitiesListAsync()
	{
		var masterlist = new List<List<object>>();
		var page = 1;
		List<Dictionary<string, object>> activities;

		do
		{
			activities = await GetStravaActivitiesAsync(page);
			foreach ( var activity in activities )
			{
				masterlist.Add(new List<object>
				{
					activity["name"], activity["id"], activity["start_date"], activity["type"]
				});
			}

			Console.WriteLine($"Received {masterlist.Count} activities");
			page++;
		} while ( activities.Count > 0 );

		ActivitiesList = masterlist;
		return masterlist;
	}

	private class MyClass
	{
		public string access_token { get; set; }
	}
	private async Task<string> RefreshAccessTokenAsync()
	{
		var client = new HttpClient();
		var tokenEndpoint = "https://www.strava.com/api/v3/oauth/token";

		var formData = new Dictionary<string, string>
		{
			{ "client_id", ClientId },
			{ "client_secret", ClientSecret },
			{ "grant_type", "refresh_token" },
			{ "refresh_token", RefreshToken }
		};

		var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(formData));
		response.EnsureSuccessStatusCode();
		var data =
			JsonSerializer.Deserialize<MyClass>(
				await response.Content.ReadAsStringAsync());
		return data.access_token;
	}

	private async Task<List<Dictionary<string, object>>> GetStravaActivitiesAsync(int page)
	{
		var client = new HttpClient();
		var apiUrl = $"https://www.strava.com/api/v3/athlete/activities?per_page=200&page={page}";
		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

		var response = await client.GetAsync(apiUrl);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
	}

	public async Task WriteToGpxAsync(long activityId, string output = "build")
	{
		var activity = await GetStravaActivityAsync(activityId);
		var gpxContentStart = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<gpx xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd"" creator=""StravaGPX"" version=""1.1"" xmlns=""http://www.topografix.com/GPX/1/1"">
  <metadata>
    <time>{activity["start_date"]}</time>
  </metadata>
  <trk>
    <name>{activity["name"]}</name>
    <type>{activity["type"]}</type>
    <trkseg>";

		var gpxContentEnd = @"
    </trkseg>
  </trk>
</gpx>";

		try
		{
			await File.WriteAllTextAsync($"{output}.gpx", gpxContentStart);

			// Logic to write trackpoints goes here (similar to Python logic, adjusted for C#).

			await File.AppendAllTextAsync($"{output}.gpx", gpxContentEnd);
			Console.WriteLine("GPX file saved successfully.");
		}
		catch ( Exception ex )
		{
			Console.WriteLine($"Error writing GPX file: {ex.Message}");
		}
	}

	private async Task<Dictionary<string, object>> GetStravaActivityAsync(long activityId)
	{
		var client = new HttpClient();
		var apiUrl =
			$"https://www.strava.com/api/v3/activities/{activityId}?include_all_efforts=false";
		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

		var response = await client.GetAsync(apiUrl);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
	}

	// Additional methods like DetectActivityStreams, AddSecondsToTimestamp, etc., can be implemented similarly.
}
