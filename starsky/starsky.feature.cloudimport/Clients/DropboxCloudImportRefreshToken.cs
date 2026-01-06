using System.Text.Json;
using starsky.foundation.cloudimport.Clients.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;

namespace starsky.foundation.cloudimport.Clients;

[Service(typeof(IDropboxCloudImportRefreshToken), InjectionLifetime = InjectionLifetime.Scoped)]
public class DropboxCloudImportRefreshToken(IHttpClientHelper httpClientHelper)
	: IDropboxCloudImportRefreshToken
{
	private const string DropboxTokenDomain = "https://api.dropbox.com/";
	private const string DropboxTokenPath = "oauth2/token";

	/// <summary>
	///     Exchanges a refresh token for a Dropbox access token using OAuth2
	/// </summary>
	public async Task<(string accessToken, int expiresIn)> ExchangeRefreshTokenAsync(
		string refreshToken, string appKey, string appSecret)
	{
		var url = new Uri(DropboxTokenDomain + DropboxTokenPath);
		var request =
			new HttpRequestMessage(HttpMethod.Post, url);
		var content = new FormUrlEncodedContent([
			new KeyValuePair<string, string>("grant_type", "refresh_token"),
			new KeyValuePair<string, string>("refresh_token", refreshToken),
			new KeyValuePair<string, string>("client_id", appKey),
			new KeyValuePair<string, string>("client_secret", appSecret)
		]);
		request.Content = content;
		var response = await httpClientHelper.PostString(url.ToString(), content);
		if ( !response.Key )
		{
			throw new HttpRequestException("Dropbox token exchange failed");
		}

		using var doc = JsonDocument.Parse(response.Value);
		var accessToken = doc.RootElement.GetProperty("access_token").GetString();
		var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var expires)
			? expires.GetInt32()
			: 14400;
		return string.IsNullOrEmpty(accessToken)
			? throw new HttpRequestException("Dropbox token exchange failed")
			: ( accessToken, expiresIn );
	}
}
