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
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.import.Services;

[Service(typeof(IImportIndexJsonService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ImportIndexJsonService(
	IImportQuery importQuery,
	AppSettings appSettings,
	ISelectorStorage selectorStorage)
	: IImportIndexJsonService
{
	private readonly IStorage _iStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	public async Task<string> ExportAsync(string outputJsonPath)
	{
		if ( string.IsNullOrWhiteSpace(outputJsonPath) )
		{
			throw new ArgumentException("Output path is required", nameof(outputJsonPath));
		}

		var directory = Path.GetDirectoryName(outputJsonPath);
		if ( !string.IsNullOrWhiteSpace(directory) )
		{
			_iStorage.CreateDirectory(directory);
		}

		var exportModel = new ImportIndexJsonContainer
		{
			ExportedAtUtc = DateTime.UtcNow,
			Version = appSettings.AppVersion,
			Structure = appSettings.Structure.Clone(),
			Items = importQuery.GetAll()
		};

		var json = JsonSerializer.Serialize(exportModel, DefaultJsonSerializer.CamelCase);
		await using var stream = StringToStreamHelper.StringToStream(json);
		await _iStorage.WriteStreamAsync(stream, outputJsonPath);

		return outputJsonPath;
	}

	public async Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath)
	{
		if ( string.IsNullOrWhiteSpace(inputJsonPath) )
		{
			throw new ArgumentException("Input path is required", nameof(inputJsonPath));
		}

		if ( !_iStorage.ExistFile(inputJsonPath) )
		{
			throw new FileNotFoundException("ImportIndex json file not found", inputJsonPath);
		}

		await using var readStream = _iStorage.ReadStream(inputJsonPath);
		using var reader = new StreamReader(readStream);
		var json = await reader.ReadToEndAsync();
		ValidateStructureAndDataSections(json);

		var importModel =
			JsonSerializer.Deserialize<ImportIndexJsonContainer>(json,
				DefaultJsonSerializer.CamelCase);
		if ( importModel?.Structure == null || importModel.Items == null )
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

			if ( await importQuery.IsHashInImportDbAsync(item.FileHash) )
			{
				item.Status = ImportStatus.IgnoredAlreadyImported;
				result.Add(item);
				continue;
			}

			item.Status = ImportStatus.Ok;
			await importQuery.AddAsync(item, false);
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
