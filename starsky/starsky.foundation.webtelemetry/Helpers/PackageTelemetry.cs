using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;

namespace starsky.foundation.webtelemetry.Helpers
{
	public class PackageTelemetry
	{
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly AppSettings _appSettings;

		public PackageTelemetry(IHttpClientHelper httpClientHelper, AppSettings appSettings)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
		}

		private const string PackageTelemetryUrl = "https://qdraw.nl/special/starsky/telemetry";
		
		public async Task Push()
		{

			OSPlatform? currentPlatform = null;
			foreach ( var platform in new List<OSPlatform>{OSPlatform.Linux, 
				         OSPlatform.Windows, OSPlatform.OSX, 
				         OSPlatform.FreeBSD}.Where(RuntimeInformation.IsOSPlatform) )
			{
				currentPlatform = platform;
			}

			var dockerContainer = currentPlatform == OSPlatform.Linux &&
			                      Environment.GetEnvironmentVariable(
				                      "DOTNET_RUNNING_IN_CONTAINER") == "true";
			
			var data = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("AppVersion", _appSettings.AppVersion),
				new KeyValuePair<string, string>("NetVersion", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription),
				new KeyValuePair<string, string>("OSVersion", System.Environment.OSVersion.Version.ToString()),
				new KeyValuePair<string, string>("OSLong", RuntimeInformation.OSDescription),
				new KeyValuePair<string, string>("OSPlatform", currentPlatform.ToString()),
				new KeyValuePair<string, string>("DockerContainer", dockerContainer.ToString()),
				new KeyValuePair<string, string>("CurrentCulture", CultureInfo.CurrentCulture.ThreeLetterISOLanguageName)
			};
			await _httpClientHelper.PostString(PackageTelemetryUrl,new FormUrlEncodedContent(data));
			
		}
	}
}

