using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starskyAdminCli.Models;

namespace starskyAdminCli.Services;

public class DropboxSetup(IConsole console, IHttpClientHelper httpClientHelper)
{
	private const string DropboxDomain = "https://www.dropbox.com";
	private const string DropboxApiDomain = "https://api.dropbox.com";

	public async Task Setup()
	{
		console.WriteLine("Dropbox Setup:");
		console.WriteLine("Go to: https://www.dropbox.com/developers/apps/create" +
		                  "\n\n-   Scoped access (1 option)" +
		                  "\n-   Select: Full Dropbox\n\n" +
		                  "In the next screen you will see and need this:\n" +
		                  "App key\n" +
		                  "App secret");

		console.WriteLine("In the Permissions tab, select the following scopes:\n" +
		                  "\nfiles.metadata.write\n" +
		                  "files.content.write\n" +
		                  "files.content.read\n" +
		                  "The next should follow \n" +
		                  "files.metadata.read\n");
		console.WriteLine("\nPress submit to save!:\n");

		Console.WriteLine("Dropbox App Key: ");
		var clientId = console.ReadLine();

		Console.WriteLine("Dropbox App Secret: ");
		var clientSecret = console.ReadLine();

		var authUrl =
			DropboxDomain + "/oauth2/authorize" +
			$"?client_id={clientId}" +
			"&response_type=code" +
			"&token_access_type=offline" +
			"&scope=" + Uri.EscapeDataString(
				"files.metadata.write " +
				"files.metadata.read " +
				"files.content.write " +
				"files.content.read"
			);

		Console.WriteLine();
		Console.WriteLine("Open this URL in your browser:");
		Console.WriteLine(authUrl);
		Console.WriteLine();
		Console.Write("Paste the access code here: ");

		var code = Console.ReadLine();
		var authHeader = Convert.ToBase64String(
			Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")
		);

		var response = await httpClientHelper.PostString(
			DropboxApiDomain + "/oauth2/token",
			new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["code"] = code!, ["grant_type"] = "authorization_code"
			}), new AuthenticationHeaderValue("Basic", authHeader));

		var token = JsonSerializer.Deserialize<DropboxTokenResponse>(response.Value)!;

		Console.WriteLine();
		Console.WriteLine("Merge this with an existing appsettings.json: ");

		var appSettingsContainer = new AppContainerAppSettings
		{
			App = new AppSettings
			{
				CloudImport = new CloudImportSettings
				{
					Providers =
					[
						new CloudImportProviderSettings
						{
							Id = "dropbox-import-example-id",
							Provider = "Dropbox",
							RemoteFolder = "/Camera Uploads",
							Enabled = true,
							Credentials = new CloudProviderCredentials
							{
								AppKey = clientId!,
								AppSecret = clientSecret!,
								RefreshToken = token.RefreshToken!
							}
						}
					]
				}
			}
		};


		Console.WriteLine(JsonSerializer.Serialize(appSettingsContainer,
			DefaultJsonSerializer.CamelCase
		));

		Console.WriteLine("Did you save the config? Press enter to exit.");
		console.ReadLine();
	}
}
