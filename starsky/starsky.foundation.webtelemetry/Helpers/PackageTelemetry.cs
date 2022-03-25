using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Attributes;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.webtelemetry.Helpers
{
	public class PackageTelemetry
	{
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;
		private readonly IQuery _query;

		public PackageTelemetry(IHttpClientHelper httpClientHelper, AppSettings appSettings, IWebLogger logger, IQuery query)
		{
			_httpClientHelper = httpClientHelper;
			_appSettings = appSettings;
			_logger = logger;
			_query = query;
		}

		internal const string PackageTelemetryUrl = "qdraw.nl/special/starsky/telemetry/index.php";

		internal static object GetPropValue(object src, string propName)
		{
			return src?.GetType().GetProperty(propName)?.GetValue(src, null);
		}

		internal static OSPlatform? GetCurrentOsPlatform()
		{
			OSPlatform? currentPlatform = null;
			foreach ( var platform in new List<OSPlatform>{OSPlatform.Linux, 
				         OSPlatform.Windows, OSPlatform.OSX, 
				         OSPlatform.FreeBSD}.Where(RuntimeInformation.IsOSPlatform) )
			{
				currentPlatform = platform;
			}

			return currentPlatform;
		}

		internal List<KeyValuePair<string, string>> GetSystemData(OSPlatform? currentPlatform = null)
		{
			currentPlatform ??= GetCurrentOsPlatform();

			var dockerContainer = currentPlatform == OSPlatform.Linux &&
			                      Environment.GetEnvironmentVariable(
				                      "DOTNET_RUNNING_IN_CONTAINER") == "true";

			var data = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("UTCTime", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
				new KeyValuePair<string, string>("AppVersion", _appSettings.AppVersion),
				new KeyValuePair<string, string>("NetVersion", RuntimeInformation.FrameworkDescription),
				new KeyValuePair<string, string>("OSArchitecture", RuntimeInformation.OSArchitecture.ToString()),
				new KeyValuePair<string, string>("ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString()),
				new KeyValuePair<string, string>("OSVersion", Environment.OSVersion.Version.ToString()),
				new KeyValuePair<string, string>("OSDescriptionLong", RuntimeInformation.OSDescription),
				new KeyValuePair<string, string>("OSPlatform", currentPlatform.ToString()),
				new KeyValuePair<string, string>("DockerContainer", dockerContainer.ToString()),
				new KeyValuePair<string, string>("CurrentCulture", CultureInfo.CurrentCulture.ThreeLetterISOLanguageName),
				new KeyValuePair<string, string>("AspNetCoreEnvironment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")),
			};
			return data;
		}

		internal async Task<List<KeyValuePair<string, string>>> AddDatabaseData(List<KeyValuePair<string, string>> data)
		{
			var fileIndexItemTotalCount = -1;
			var fileIndexItemDirectoryCount = -1;
			var fileIndexItemCount = -1;
			
			try
			{
				fileIndexItemTotalCount = await _query.CountAsync();
				fileIndexItemDirectoryCount = await _query.CountAsync(p => p.IsDirectory == true);
				fileIndexItemCount = await _query.CountAsync(p => p.IsDirectory != true);
			}
			catch ( Exception )
			{
				// ignored nothing here
			}

			data.AddRange(new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("FileIndexItemTotalCount",fileIndexItemTotalCount.ToString()),
				new KeyValuePair<string, string>("FileIndexItemDirectoryCount",fileIndexItemDirectoryCount.ToString()),
				new KeyValuePair<string, string>("FileIndexItemCount",fileIndexItemCount.ToString())
			});
			
			return data;
		}

		internal List<KeyValuePair<string, string>> AddAppSettingsData( List<KeyValuePair<string, string>> data)
		{
			var type = typeof(AppSettings);
			var properties = type.GetProperties();
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var property in properties)
			{
				var someAttribute = Attribute.GetCustomAttributes(property).FirstOrDefault(x => x is PackageTelemetryAttribute);
				if ( someAttribute == null )
				{
					continue;
				}

				var propValue = GetPropValue(_appSettings.CloneToDisplay(),
					property.Name);
				var value = propValue?.ToString();

				if ( propValue?.GetType() == typeof(DateTime) )
				{
					value = ((DateTime)propValue).ToString(CultureInfo.InvariantCulture);
				}
				
				if (propValue?.GetType() == typeof(List<string>) || 
				    propValue?.GetType() == typeof(Dictionary<string, List<AppSettingsPublishProfiles>>) )
				{
					value = ParseContent(propValue);
				}

				data.Add(new KeyValuePair<string, string>("AppSettings" + property.Name, value));
			}
			return data;
		}

		internal static string ParseContent(object propValue)
		{
			return JsonSerializer.Serialize(propValue);
		}

		private async Task<bool> PostData(HttpContent formUrlEncodedContent)
		{
			return (await _httpClientHelper.PostString("https://" + PackageTelemetryUrl,formUrlEncodedContent, 
				_appSettings.EnablePackageTelemetryDebug == true)).Key;
		}
		
		public async Task<bool?> PackageTelemetrySend()
		{
			if ( _appSettings.EnablePackageTelemetry == false )
			{
				return null;
			}
			
			var telemetryDataItems = GetSystemData();
			telemetryDataItems = AddAppSettingsData(telemetryDataItems);
			telemetryDataItems = await AddDatabaseData(telemetryDataItems);

			var formEncodedData = new FormUrlEncodedContent(telemetryDataItems);

			if ( _appSettings.EnablePackageTelemetryDebug != true )
				return await PostData(formEncodedData);
			
			foreach ( var (key, value) in telemetryDataItems )
			{
				_logger.LogInformation($"[EnablePackageTelemetryDebug] {key} - {value}");
			}
			return null;


		}
	}
}

