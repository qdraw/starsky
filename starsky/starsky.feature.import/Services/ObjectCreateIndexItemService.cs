using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Structure;
using starsky.foundation.storage.Structure.Helpers;

namespace starsky.feature.import.Services;

public class ObjectCreateIndexItemService(
	AppSettings appSettings,
	IReverseGeoCodeService reverseGeoCode)
{
	/// <summary>
	///     Used when File has no exif date in description
	/// </summary>
	internal const string MessageDateTimeBasedOnFilename = "Date and Time based on filename";

	public ImportIndexItem CreateObjectIndexItem(KeyValuePair<string, bool> inputFileFullPath,
		FileIndexItem? fileIndexItem, KeyValuePair<string, bool> hashList, long size,
		int importSettingsColorClass)
	{
		var importIndexItem = new ImportIndexItem(appSettings)
		{
			SourceFullFilePath = inputFileFullPath.Key,
			DateTime = fileIndexItem?.DateTime ?? DateTime.MinValue,
			FileHash = hashList.Key,
			FileIndexItem = fileIndexItem,
			Status = ImportStatus.Ok,
			FilePath = fileIndexItem?.FilePath,
			ColorClass = fileIndexItem?.ColorClass ?? ColorClassParser.Color.DoNotChange,
			ImageFormat =
				fileIndexItem?.ImageFormat ?? ExtensionRolesHelper.ImageFormat.unknown,
			MakeModel = fileIndexItem?.MakeModel ?? string.Empty,
			Size = size
		};

		// Overwrite ColorClass when set in ImportSettingsModel
		if ( importSettingsColorClass < 0 )
		{
			return importIndexItem;
		}

		// only when set in ImportSettingsModel
		var colorClass = ( ColorClassParser.Color ) importSettingsColorClass;
		importIndexItem.FileIndexItem!.ColorClass = colorClass;
		importIndexItem.ColorClass = colorClass;
		return importIndexItem;
	}

	/// <summary>
	///     Create a new import object
	/// </summary>
	/// <param name="importIndexItem"></param>
	/// <param name="importSettingsReverseGeoCode"></param>
	/// <returns></returns>
	public async Task<ImportIndexItem> TransformCreateIndexItem(ImportIndexItem importIndexItem,
		bool importSettingsReverseGeoCode, string overwriteStructure = "")
	{
		UpdateForWithoutExif(importIndexItem.FileIndexItem!, importIndexItem, overwriteStructure);

		// AddToDatabase is Used by the importer History agent
		importIndexItem.FileIndexItem!.AddToDatabase = DateTime.UtcNow;
		importIndexItem.AddToDatabase = DateTime.UtcNow;
		importIndexItem.FileIndexItem.Size = importIndexItem.Size;
		importIndexItem.FileIndexItem.FileHash = importIndexItem.FileHash;
		importIndexItem.FileIndexItem.ImageFormat = importIndexItem.ImageFormat;
		importIndexItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;

		if ( !importSettingsReverseGeoCode )
		{
			return importIndexItem;
		}

		var location = await reverseGeoCode.GetLocation(importIndexItem.FileIndexItem.Latitude,
			importIndexItem.FileIndexItem.Longitude);
		if ( !location.IsSuccess )
		{
			return importIndexItem;
		}

		importIndexItem.FileIndexItem.LocationCity = location.LocationCity;
		importIndexItem.FileIndexItem.LocationCountry = location.LocationCountry;
		importIndexItem.FileIndexItem.LocationCountryCode = location.LocationCountryCode;
		importIndexItem.FileIndexItem.LocationState = location.LocationState;
		return importIndexItem;
	}

	private void UpdateForWithoutExif(FileIndexItem fileIndexItem,
		ImportIndexItem importIndexItem, string overwriteStructure)
	{
		// used for files without an Exif Date for example WhatsApp images
		if ( fileIndexItem.DateTime.Year != 1 )
		{
			return;
		}

		var inputModel = new StructureInputModel(
			fileIndexItem.DateTime, importIndexItem.FileIndexItem!.FileCollectionName!,
			FilenamesHelper.GetFileExtensionWithoutDot(importIndexItem.FileIndexItem
				.FileName!),
			importIndexItem.ImageFormat);

		var structureObject = CreateStructure(overwriteStructure);
		var helper = new ParseDateTimeFromFileNameHelper(structureObject);
		var dateTimeFromFileName = helper.ParseDateTimeFromFileName(inputModel);

		importIndexItem.FileIndexItem.DateTime = dateTimeFromFileName;
		importIndexItem.DateTime = dateTimeFromFileName;

		// used to sync exifTool and to let the user know that the transformation has been applied
		importIndexItem.FileIndexItem.Description = MessageDateTimeBasedOnFilename;
		// only set when date is parsed if not ignore update
		if ( importIndexItem.FileIndexItem.DateTime.Year != 1 )
		{
			importIndexItem.DateTimeFromFileName = true;
		}
	}

	private AppSettingsStructureModel CreateStructure(string overwriteStructure)
	{
		var structure = appSettings.Structure.CloneViaJson();
		structure ??= new AppSettingsStructureModel();

		structure.OverrideDefaultPatternAndDisableRules(overwriteStructure,
			[]);
		return structure;
	}
}
