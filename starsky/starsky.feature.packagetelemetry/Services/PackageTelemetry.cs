using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Attributes;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.packagetelemetry.Services;

[Service(typeof(IPackageTelemetry), InjectionLifetime = InjectionLifetime.Scoped)]
public class PackageTelemetry : IPackageTelemetry
{
	internal const string PackageTelemetryUrl = "qdraw.nl/special/starsky/telemetry/index.php";

	private readonly AppSettings _appSettings;
	private readonly IDeviceIdService _deviceIdService;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly ILifetimeDiagnosticsService _lifetimeDiagnosticsService;
	private readonly IWebLogger _logger;
	private readonly IQuery _query;

	public PackageTelemetry(IHttpClientHelper httpClientHelper, AppSettings appSettings,
		IWebLogger logger, IQuery query, IDeviceIdService deviceIdService,
		ILifetimeDiagnosticsService lifetimeDiagnosticsService)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_logger = logger;
		_query = query;
		_deviceIdService = deviceIdService;
		_lifetimeDiagnosticsService = lifetimeDiagnosticsService;
	}

	public async Task<bool?> PackageTelemetrySend()
	{
		if ( _appSettings.EnablePackageTelemetry == false )
		{
			return null;
		}

		var currentOsPlatform = GetCurrentOsPlatform();
		var deviceId = await _deviceIdService.DeviceId(currentOsPlatform);

		var telemetryDataItems = GetSystemData(currentOsPlatform, deviceId);
		telemetryDataItems = AddAppSettingsData(telemetryDataItems);
		telemetryDataItems = await AddDatabaseData(telemetryDataItems);

		var formEncodedData = new FormUrlEncodedContent(telemetryDataItems);

		if ( _appSettings.EnablePackageTelemetryDebug != true )
		{
			var postResult = await PostData(formEncodedData);
			if ( !postResult )
			{
				_logger.LogError($"[PackageTelemetry] postResult: {postResult} - " +
				                 $"Url: {PackageTelemetryUrl} - " +
				                 $"Form: {formEncodedData}");
			}

			return postResult;
		}

		foreach ( var (key, value) in telemetryDataItems )
		{
			_logger.LogInformation($"[EnablePackageTelemetryDebug] {key} - {value}");
		}

		return null;
	}

	internal static object? GetPropValue(object? src, string propName)
	{
		return src?.GetType().GetProperty(propName)?.GetValue(src, null);
	}

	internal static OSPlatform? GetCurrentOsPlatform()
	{
		OSPlatform? currentPlatform = null;
		foreach ( var platform in new List<OSPlatform>
		         {
			         OSPlatform.Linux, OSPlatform.Windows, OSPlatform.OSX, OSPlatform.FreeBSD
		         }.Where(RuntimeInformation.IsOSPlatform) )
		{
			currentPlatform = platform;
		}

		return currentPlatform;
	}

	internal List<KeyValuePair<string, string>> GetSystemData(OSPlatform? currentPlatform = null,
		string? deviceId = null)
	{
		currentPlatform ??= GetCurrentOsPlatform();

		var dockerContainer = currentPlatform == OSPlatform.Linux &&
		                      Environment.GetEnvironmentVariable(
			                      "DOTNET_RUNNING_IN_CONTAINER") == "true";

		var data = new List<KeyValuePair<string, string>>
		{
			new("UTCTime", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
			new("AppVersion", _appSettings.AppVersion),
			new("NetVersion", RuntimeInformation.FrameworkDescription),
			new("OSArchitecture", RuntimeInformation.OSArchitecture.ToString()),
			new("ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString()),
			new("OSVersion", Environment.OSVersion.Version.ToString()),
			new("OSDescriptionLong", RuntimeInformation.OSDescription.Replace(";", " ")),
			new("OSPlatform", currentPlatform.ToString()!),
			new("DockerContainer", dockerContainer.ToString()),
			new("CurrentCulture", CultureInfo.CurrentCulture.ThreeLetterISOLanguageName),
			new("AspNetCoreEnvironment",
				Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set"),
			new("WebsiteName", GetEncryptedSiteName()),
			new("DeviceId", deviceId ?? "Not set")
		};
		return data;
	}

	private static string GetEncryptedSiteName()
	{
		var siteName =
			Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
		return string.IsNullOrEmpty(siteName)
			? "Not set"
			: Sha256.ComputeSha256(siteName); // used in Azure web apps
	}


	internal async Task<List<KeyValuePair<string, string>>> AddDatabaseData(
		List<KeyValuePair<string, string>> data)
	{
		var fileIndexItemTotalCount = -1;
		var fileIndexItemDirectoryCount = -1;
		var fileIndexItemCount = -1;
		double lastApplicationStopping = -1;

		try
		{
			fileIndexItemTotalCount = await _query.CountAsync();
			fileIndexItemDirectoryCount = await _query.CountAsync(p => p.IsDirectory == true);
			fileIndexItemCount = await _query.CountAsync(p => p.IsDirectory != true);
			lastApplicationStopping =
				await _lifetimeDiagnosticsService.GetLastApplicationStoppingTimeInMinutes();
		}
		catch ( Exception )
		{
			// ignored nothing here
		}

		data.AddRange(new List<KeyValuePair<string, string>>
		{
			new("FileIndexItemTotalCount", fileIndexItemTotalCount.ToString()),
			new("FileIndexItemDirectoryCount", fileIndexItemDirectoryCount.ToString()),
			new("FileIndexItemCount", fileIndexItemCount.ToString()),
			new("LastApplicationStoppingLifetimeInMinutes",
				lastApplicationStopping.ToString(CultureInfo.InvariantCulture))
		});

		return data;
	}

	internal List<KeyValuePair<string, string>> AddAppSettingsData(
		List<KeyValuePair<string, string>> data)
	{
		var type = typeof(AppSettings);
		var properties = type.GetProperties();
		// ReSharper disable once LoopCanBeConvertedToQuery
		foreach ( var property in properties )
		{
			var someAttribute = Array.Find(Attribute.GetCustomAttributes(property),
				x => x is PackageTelemetryAttribute);
			if ( someAttribute == null )
			{
				continue;
			}

			var propValue = GetPropValue(_appSettings.CloneToDisplay(),
				property.Name);
			var value = propValue?.ToString();

			if ( propValue?.GetType() == typeof(DateTime) )
			{
				value = ( ( DateTime ) propValue ).ToString(CultureInfo.InvariantCulture);
			}

			if ( propValue?.GetType() == typeof(List<string>) ||
			     propValue?.GetType() ==
			     typeof(Dictionary<string, List<AppSettingsPublishProfiles>>) ||
			     propValue?.GetType() == typeof(List<CameraMakeModel>) ||
			     propValue?.GetType() == typeof(List<AppSettingsDefaultEditorApplication>) ||
			     propValue?.GetType() == typeof(AppSettingsImportTransformationModel) ||
			     propValue?.GetType() == typeof(AppSettingsStructureModel) )
			{
				value = ParseContentToJson(propValue);
			}

			data.Add(new KeyValuePair<string, string>("AppSettings" + property.Name,
				value ?? "null"));
		}

		return data;
	}

	internal static string ParseContentToJson(object propValue)
	{
		return JsonSerializer.Serialize(propValue);
	}

	/// <summary>
	///     Send data to service
	/// </summary>
	/// <param name="formUrlEncodedContent">data to send to</param>
	/// <returns>if success</returns>
	private async Task<bool> PostData(HttpContent formUrlEncodedContent)
	{
		return ( await _httpClientHelper.PostString("https://" + PackageTelemetryUrl,
			formUrlEncodedContent,
			_appSettings.EnablePackageTelemetryDebug == true) ).Key;
	}
}
