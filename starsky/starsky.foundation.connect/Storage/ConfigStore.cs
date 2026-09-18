using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace starsky.foundation.connect.Storage;

/// <summary>
/// Persists the Connect engine configuration (cert, folders, known devices)
/// as a JSON file in the application data directory.
/// </summary>
public sealed class ConfigStore
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	private readonly string _filePath;

	public ConfigStore(string filePath)
	{
		_filePath = filePath;
	}

	public static ConfigStore Default()
	{
		var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var path = Path.Combine(appData, "starsky", "connect-config.json");
		return new ConfigStore(path);
	}

	public async Task<ConnectConfig> LoadAsync()
	{
		if ( !File.Exists(_filePath) )
		{
			return new ConnectConfig();
		}

		await using var stream = File.OpenRead(_filePath);
		return await JsonSerializer.DeserializeAsync<ConnectConfig>(stream, JsonOptions)
		       ?? new ConnectConfig();
	}

	public async Task SaveAsync(ConnectConfig config)
	{
		var dir = Path.GetDirectoryName(_filePath);
		if ( dir is not null )
		{
			Directory.CreateDirectory(dir);
		}

		await using var stream = File.Create(_filePath);
		await JsonSerializer.SerializeAsync(stream, config, JsonOptions);
	}
}

public sealed class ConnectConfig
{
	public string DeviceName { get; set; } = Environment.MachineName;

	/// <summary>PFX-encoded self-signed certificate (Base64). Generated on first run.</summary>
	public string? CertificatePfxBase64 { get; set; }

	public List<ConnectFolderConfig> Folders { get; set; } = [];

	public List<ConnectDeviceConfig> Devices { get; set; } = [];
}

public sealed class ConnectFolderConfig
{
	public string Id { get; set; } = string.Empty;
	public string Label { get; set; } = string.Empty;
	public string Path { get; set; } = string.Empty;
	public string Type { get; set; } = "SendReceive";
}

public sealed class ConnectDeviceConfig
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public List<string> Addresses { get; set; } = [];
}
