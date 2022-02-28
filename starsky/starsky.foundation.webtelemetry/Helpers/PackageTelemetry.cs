using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

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

		private static object GetPropValue(object src, string propName)
		{
			return src?.GetType().GetProperty(propName)?.GetValue(src, null);
		}

		private static OSPlatform? GetCurrentOsPlatform()
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

		private List<KeyValuePair<string, string>> GetSystemData()
		{
			var currentPlatform = GetCurrentOsPlatform();

			var dockerContainer = currentPlatform == OSPlatform.Linux &&
			                      Environment.GetEnvironmentVariable(
				                      "DOTNET_RUNNING_IN_CONTAINER") == "true";
			
			var buildDate = DateAssembly.GetBuildDate(Assembly.GetExecutingAssembly()).ToString(
				new CultureInfo("nl-NL"));
			
			var data = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("AppVersion", _appSettings.AppVersion),
				new KeyValuePair<string, string>("NetVersion", RuntimeInformation.FrameworkDescription),
				new KeyValuePair<string, string>("OSArchitecture", RuntimeInformation.OSArchitecture.ToString()),
				new KeyValuePair<string, string>("ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString()),
				new KeyValuePair<string, string>("OSVersion", Environment.OSVersion.Version.ToString()),
				new KeyValuePair<string, string>("OSDescriptionLong", RuntimeInformation.OSDescription),
				new KeyValuePair<string, string>("OSPlatform", currentPlatform.ToString()),
				new KeyValuePair<string, string>("DockerContainer", dockerContainer.ToString()),
				new KeyValuePair<string, string>("CurrentCulture", CultureInfo.CurrentCulture.ThreeLetterISOLanguageName),
				new KeyValuePair<string, string>("BuildDate", buildDate),
				new KeyValuePair<string, string>("AspNetCoreEnvironment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")),
			};
			return data;
		}
		
		private List<KeyValuePair<string, string>> AddAppSettingsData( List<KeyValuePair<string, string>> data)
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
				var value = GetPropValue(_appSettings.CloneToDisplay(), property.Name)?.ToString();
				data.Add(new KeyValuePair<string, string>("AppSettings" + property.Name, value));
			}
			return data;
		}
		
		public async Task Push()
		{
			if ( _appSettings.EnablePackageTelemetry == false )
			{
				return;
			}
			
			var data = GetSystemData();
			data = AddAppSettingsData(data);
			var result = await _httpClientHelper.PostString(PackageTelemetryUrl,new FormUrlEncodedContent(data));
		}
	}
}

