using System;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.platform.Helpers;
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


	/// <summary>
	///     Create a new import object
	/// </summary>
	/// <param name="inputFileFullPath">full file path</param>
	/// <param name="imageFormat">is it jpeg or png or something different</param>
	/// <param name="fileHashCode">file hash base32</param>
	/// <param name="fileIndexItem">database item</param>
	/// <param name="colorClassTransformation">Force to update colorClass</param>
	/// <param name="size">Add filesize in bytes</param>
	/// <param name="importSettingsReverseGeoCode"></param>
	/// <returns></returns>
	public async Task<ImportIndexItem> ObjectCreateIndexItem(string inputFileFullPath,
		ExtensionRolesHelper.ImageFormat imageFormat,
		string fileHashCode,
		FileIndexItem fileIndexItem,
		int colorClassTransformation,
		long size, bool importSettingsReverseGeoCode)
	{
		var importIndexItem = new ImportIndexItem(appSettings)
		{
			SourceFullFilePath = inputFileFullPath,
			DateTime = fileIndexItem.DateTime,
			FileHash = fileHashCode,
			FileIndexItem = fileIndexItem,
			Status = ImportStatus.Ok,
			FilePath = fileIndexItem.FilePath,
			ColorClass = fileIndexItem.ColorClass
		};

		// used for files without an Exif Date for example WhatsApp images
		if ( fileIndexItem.DateTime.Year == 1 )
		{
			var inputModel = new StructureInputModel(
				fileIndexItem.DateTime, importIndexItem.FileIndexItem.FileCollectionName!,
				FilenamesHelper.GetFileExtensionWithoutDot(importIndexItem.FileIndexItem
					.FileName!),
				imageFormat);
			var helper = new ParseDateTimeFromFileNameHelper(appSettings);
			importIndexItem.FileIndexItem.DateTime = helper.ParseDateTimeFromFileName(inputModel);

			// used to sync exifTool and to let the user know that the transformation has been applied
			importIndexItem.FileIndexItem.Description = MessageDateTimeBasedOnFilename;
			// only set when date is parsed if not ignore update
			if ( importIndexItem.FileIndexItem.DateTime.Year != 1 )
			{
				importIndexItem.DateTimeFromFileName = true;
			}
		}

		// Also add Camera brand to list
		importIndexItem.MakeModel = importIndexItem.FileIndexItem.MakeModel;

		// AddToDatabase is Used by the importer History agent
		importIndexItem.FileIndexItem.AddToDatabase = DateTime.UtcNow;
		importIndexItem.AddToDatabase = DateTime.UtcNow;

		importIndexItem.FileIndexItem.Size = size;
		importIndexItem.FileIndexItem.FileHash = fileHashCode;
		importIndexItem.FileIndexItem.ImageFormat = imageFormat;
		importIndexItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;

		if ( importSettingsReverseGeoCode )
		{
			var location = await reverseGeoCode.GetLocation(importIndexItem.FileIndexItem.Latitude,
				importIndexItem.FileIndexItem.Longitude);
			if ( location.IsSuccess )
			{
				importIndexItem.FileIndexItem.LocationCity = location.LocationCity;
				importIndexItem.FileIndexItem.LocationCountry = location.LocationCountry;
				importIndexItem.FileIndexItem.LocationCountryCode = location.LocationCountryCode;
				importIndexItem.FileIndexItem.LocationState = location.LocationState;
			}
		}

		if ( colorClassTransformation < 0 )
		{
			return importIndexItem;
		}

		// only when set in ImportSettingsModel
		var colorClass = ( ColorClassParser.Color ) colorClassTransformation;
		importIndexItem.FileIndexItem.ColorClass = colorClass;
		importIndexItem.ColorClass = colorClass;
		return importIndexItem;
	}
}
