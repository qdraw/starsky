using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starsky.foundation.import.Services;

[Service(typeof(IImportIndexJsonService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ImportIndexJsonService : IImportIndexJsonService
{
	private readonly AppSettings _appSettings;
	private readonly IImportQuery _importQuery;

	public ImportIndexJsonService(IImportQuery importQuery, AppSettings appSettings)
	{
		_importQuery = importQuery;
		_appSettings = appSettings;
	}

	public async Task<string> ExportAsync(string outputJsonPath)
	{
		if ( string.IsNullOrWhiteSpace(outputJsonPath) )
		{
			throw new ArgumentException("Output path is required", nameof(outputJsonPath));
		}

		var directory = Path.GetDirectoryName(outputJsonPath);
		if ( !string.IsNullOrWhiteSpace(directory) )
		{
			Directory.CreateDirectory(directory);
		}

		var exportModel = new ImportIndexJsonContainer
		{
			ExportedAtUtc = DateTime.UtcNow,
			Version = _appSettings.AppVersion,
			Structure = _appSettings.Structure.Clone(),
			Items = _importQuery.GetAll()
		};

		var json = JsonSerializer.Serialize(exportModel, DefaultJsonSerializer.CamelCase);
		await File.WriteAllTextAsync(outputJsonPath, json);

		return outputJsonPath;
	}

	public async Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath)
	{
		if ( string.IsNullOrWhiteSpace(inputJsonPath) )
		{
			throw new ArgumentException("Input path is required", nameof(inputJsonPath));
		}

		if ( !File.Exists(inputJsonPath) )
		{
			throw new FileNotFoundException("ImportIndex json file not found", inputJsonPath);
		}

		var json = await File.ReadAllTextAsync(inputJsonPath);
		ValidateStructureAndDataSections(json);

		var importModel = JsonSerializer.Deserialize<ImportIndexJsonContainer>(json,
			DefaultJsonSerializer.CamelCase);

		if ( importModel == null )
		{
			throw new InvalidDataException("Failed to deserialize ImportIndex json container");
		}

		var result = new List<ImportIndexItem>();
		foreach ( var item in importModel.Items )
		{
			if ( string.IsNullOrWhiteSpace(item.FileHash) )
			{
				item.Status = ImportStatus.FileError;
				result.Add(item);
				continue;
			}

			if ( await _importQuery.IsHashInImportDbAsync(item.FileHash) )
			{
				item.Status = ImportStatus.IgnoredAlreadyImported;
				result.Add(item);
				continue;
			}

			item.Status = ImportStatus.Ok;
			await _importQuery.AddAsync(item, false);
			result.Add(item);
		}

		return result;
	}

	private static void ValidateStructureAndDataSections(string json)
	{
		using var jsonDocument = JsonDocument.Parse(json);
		if ( !jsonDocument.RootElement.TryGetProperty("structure", out _) ||
		     !jsonDocument.RootElement.TryGetProperty("items", out _) )
		{
			throw new InvalidDataException(
				"ImportIndex json should contain both 'structure' and 'items' sections");
		}
	}
}