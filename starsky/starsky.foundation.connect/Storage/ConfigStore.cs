using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.settings.Enums;

namespace starsky.foundation.connect.Storage;

/// <summary>
/// Persists the Connect engine configuration (cert, folders, known devices).
///
/// The TLS certificate is kept in the <see cref="ApplicationDbContext"/> Settings table so
/// it survives Docker container restarts and volume wipes. The JSON file on disk is a
/// local cache: it is written on every save so the engine can start quickly without hitting
/// the database, and it holds the folder/device list that is not in the Settings table.
/// </summary>
public sealed class ConfigStore
{
	private static readonly string DbCertKey =
		Enum.GetName(SettingsType.ConnectCertificatePfxBase64)!;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	private readonly string _filePath;
	private readonly ApplicationDbContext? _db;

	public ConfigStore(string filePath, ApplicationDbContext? db = null)
	{
		_filePath = filePath;
		_db = db;
	}

	public static ConfigStore Default(ApplicationDbContext? db = null)
	{
		var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var path = Path.Combine(appData, "starsky", "connect-config.json");
		return new ConfigStore(path, db);
	}

	/// <summary>
	/// Loads config. The certificate comes from the DB when available (Docker-safe);
	/// folders and devices come from the JSON file.
	/// </summary>
	public async Task<ConnectConfig> LoadAsync()
	{
		// Load the file first (folders, devices, and the cached cert).
		ConnectConfig config;
		if ( File.Exists(_filePath) )
		{
			await using var stream = File.OpenRead(_filePath);
			config = await JsonSerializer.DeserializeAsync<ConnectConfig>(stream, JsonOptions)
			         ?? new ConnectConfig();
		}
		else
		{
			config = new ConnectConfig();
		}

		// DB is authoritative for the cert. Override whatever the file had.
		if ( _db is not null )
		{
			var row = await _db.Settings.AsNoTracking()
				.FirstOrDefaultAsync(s => s.Key == DbCertKey);

			if ( row is not null && !string.IsNullOrEmpty(row.Value) )
			{
				config.CertificatePfxBase64 = row.Value;

				// Keep the file cache in sync so future cold starts are fast.
				await WriteCacheAsync(config);
			}
		}

		return config;
	}

	/// <summary>
	/// Saves config. The certificate is written to the DB (survives Docker restarts)
	/// and the full config is written to the JSON file as a local cache.
	/// </summary>
	public async Task SaveAsync(ConnectConfig config)
	{
		if ( _db is not null && !string.IsNullOrEmpty(config.CertificatePfxBase64) )
		{
			var existing = await _db.Settings
				.FirstOrDefaultAsync(s => s.Key == DbCertKey);

			if ( existing is null )
			{
				_db.Settings.Add(new SettingsItem
				{
					Key = DbCertKey,
					Value = config.CertificatePfxBase64,
				});
			}
			else
			{
				existing.Value = config.CertificatePfxBase64;
				_db.Settings.Update(existing);
			}

			await _db.SaveChangesAsync();
		}

		await WriteCacheAsync(config);
	}

	private async Task WriteCacheAsync(ConnectConfig config)
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

	/// <summary>PFX-encoded self-signed certificate (Base64). Stored in the DB Settings table; cached here.</summary>
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
