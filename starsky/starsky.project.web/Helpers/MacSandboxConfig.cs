using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.project.web.Helpers;

/// <summary>
/// Reads the startup config written by the macOS host app into the App Group container
/// and applies it as environment variables before the ASP.NET host is built.
/// Only active when running as a sandboxed MAS Login Item
/// (indicated by the STARSKY_APP_GROUP environment variable set via LSEnvironment).
/// </summary>
public static class MacSandboxConfig
{
	private const string AppGroupEnvKey = "STARSKY_APP_GROUP";
	private const string ConfigFileName = "backend-config.json";

	/// <summary>
	/// Reads backend-config.json from the App Group container and sets the
	/// corresponding environment variables. Returns true when config was applied.
	/// </summary>
	public static bool TryApply()
	{
		if ( !OperatingSystem.IsMacOS() )
		{
			return false;
		}

		var appGroup = Environment.GetEnvironmentVariable(AppGroupEnvKey);
		if ( string.IsNullOrEmpty(appGroup) )
		{
			return false;
		}

		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var groupContainer = Path.Combine(home, "Library", "Group Containers", appGroup);
		var configPath = Path.Combine(groupContainer, ConfigFileName);

		if ( !File.Exists(configPath) )
		{
			return false;
		}

		BackendConfig? config;
		try
		{
			var json = File.ReadAllText(configPath);
			config = JsonSerializer.Deserialize<BackendConfig>(json,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		}
		catch
		{
			return false;
		}

		if ( config is null )
		{
			return false;
		}

		ApplyConfig(config);
		return true;
	}

	internal static void ApplyConfig(BackendConfig config)
	{
		SetIfNotEmpty("ASPNETCORE_URLS", config.AspNetCoreUrls);
		SetIfNotEmpty("app__appsettingspath", config.AppSettingsPath);
		SetIfNotEmpty("app__appsettingslocalpath", config.AppSettingsLocalPath);
		SetIfNotEmpty("app__databaseConnection", config.DatabaseConnection);
		SetIfNotEmpty("app__tempFolder", config.TempFolder);
		SetIfNotEmpty("app__thumbnailTempFolder", config.ThumbnailTempFolder);
		Environment.SetEnvironmentVariable("app__NoAccountLocalhost", "true");
		Environment.SetEnvironmentVariable("app__UseLocalDesktop", "true");
		Environment.SetEnvironmentVariable("app__AccountRegisterDefaultRole", "Administrator");
		Environment.SetEnvironmentVariable("app__ThumbnailGenerationIntervalInMinutes", "300");
		Environment.SetEnvironmentVariable("app__Verbose", "false");
	}

	private static void SetIfNotEmpty(string key, string? value)
	{
		if ( !string.IsNullOrEmpty(value) )
		{
			Environment.SetEnvironmentVariable(key, value);
		}
	}
}

internal sealed class BackendConfig
{
	[JsonPropertyName("aspNetCoreUrls")]
	public string? AspNetCoreUrls { get; set; }

	[JsonPropertyName("appSettingsPath")]
	public string? AppSettingsPath { get; set; }

	[JsonPropertyName("appSettingsLocalPath")]
	public string? AppSettingsLocalPath { get; set; }

	[JsonPropertyName("databaseConnection")]
	public string? DatabaseConnection { get; set; }

	[JsonPropertyName("tempFolder")]
	public string? TempFolder { get; set; }

	[JsonPropertyName("thumbnailTempFolder")]
	public string? ThumbnailTempFolder { get; set; }

	[JsonPropertyName("logsDirectory")]
	public string? LogsDirectory { get; set; }
}
