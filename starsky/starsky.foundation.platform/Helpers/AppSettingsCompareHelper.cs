using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers.Compare;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Helpers;

public static class AppSettingsCompareHelper
{
	/// <summary>
	///     Compare a fileIndex item and update items if there are changed in the updateObject
	///     append => (propertyName == "Tags" add it with comma space or with single space)
	/// </summary>
	/// <param name="sourceIndexItem">the source object</param>
	/// <param name="updateObject">the item with changed values</param>
	/// <returns>list of changed types</returns>
	public static List<string> Compare(AppSettings sourceIndexItem, object? updateObject = null)
	{
		updateObject ??= new AppSettings();
		var propertiesA = sourceIndexItem.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var propertiesB = updateObject.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		var differenceList = new List<string>();
		foreach ( var propertyB in propertiesB )
		{
			// only for when TransferObject is Nullable<bool> and AppSettings is bool
			var propertyInfoFromA = Array.Find(propertiesA, p => p.Name == propertyB.Name);
			if ( propertyInfoFromA == null )
			{
				continue;
			}

			CompareMultipleSingleItems(propertyB, propertyInfoFromA, sourceIndexItem,
				updateObject, differenceList);
			CompareMultipleListDictionary(propertyB, propertyInfoFromA, sourceIndexItem,
				updateObject, differenceList);
			CompareListMultipleObjects(propertyB, propertyInfoFromA, sourceIndexItem,
				updateObject,
				differenceList);
		}

		return differenceList;
	}

	private static void CompareListMultipleObjects(PropertyInfo propertyB,
		PropertyInfo propertyInfoFromA, AppSettings sourceIndexItem, object updateObject,
		List<string> differenceList)
	{
		if ( propertyInfoFromA.PropertyType == typeof(OpenTelemetrySettings) &&
		     propertyB.PropertyType == typeof(OpenTelemetrySettings) )
		{
			var oldObjectValue =
				( OpenTelemetrySettings? ) propertyInfoFromA.GetValue(sourceIndexItem, null);
			var newObjectValue =
				( OpenTelemetrySettings? ) propertyB.GetValue(updateObject, null);
			CompareOpenTelemetrySettingsObject(propertyB.Name, sourceIndexItem, oldObjectValue,
				newObjectValue, differenceList);
		}

		if ( propertyB.PropertyType == typeof(AppSettingsStructureModel) )
		{
			var oldStructure =
				( AppSettingsStructureModel? ) propertyInfoFromA.GetValue(sourceIndexItem,
					null);
			var newStructure =
				( AppSettingsStructureModel? ) propertyB.GetValue(updateObject, null);
			CompareAppSettingsStructureModel(propertyB.Name, sourceIndexItem,
				oldStructure,
				newStructure, differenceList);
		}

		if ( propertyB.PropertyType == typeof(AppSettingsImportTransformationModel) )
		{
			var oldImportTransformation =
				( AppSettingsImportTransformationModel? ) propertyInfoFromA.GetValue(
					sourceIndexItem,
					null);
			var newImportTransformation =
				( AppSettingsImportTransformationModel? ) propertyB.GetValue(updateObject, null);
			CompareAppSettingsImportTransformationModel(propertyB.Name, sourceIndexItem,
				oldImportTransformation,
				newImportTransformation, differenceList);
		}

		if ( propertyInfoFromA.PropertyType ==
		     typeof(List<AppSettingsDefaultEditorApplication>) &&
		     propertyB.PropertyType == typeof(List<AppSettingsDefaultEditorApplication>) )
		{
			var oldObjectValue =
				( List<AppSettingsDefaultEditorApplication>? ) propertyInfoFromA.GetValue(
					sourceIndexItem, null);
			var newObjectValue =
				( List<AppSettingsDefaultEditorApplication>? ) propertyB.GetValue(updateObject,
					null);
			CompareAppSettingsDefaultEditorApplication(propertyB.Name, sourceIndexItem,
				oldObjectValue,
				newObjectValue, differenceList);
		}
	}

	[SuppressMessage("Performance",
		"CA1859:Use concrete types when possible for improved performance")]
	private static void CompareOpenTelemetrySettingsObject(string propertyName,
		AppSettings? sourceIndexItem,
		OpenTelemetrySettings? oldKeyValuePairStringStringValue,
		OpenTelemetrySettings? newKeyValuePairStringStringValue,
		ICollection<string> differenceList)
	{
		if ( oldKeyValuePairStringStringValue == null ||
		     newKeyValuePairStringStringValue == null ||
		     // compare lists
		     JsonSerializer.Serialize(oldKeyValuePairStringStringValue) ==
		     JsonSerializer.Serialize(newKeyValuePairStringStringValue) ||
		     // default options
		     JsonSerializer.Serialize(newKeyValuePairStringStringValue) ==
		     JsonSerializer.Serialize(new OpenTelemetrySettings()) )
		{
			return;
		}

		sourceIndexItem!.GetType().GetProperty(propertyName)!.SetValue(sourceIndexItem,
			newKeyValuePairStringStringValue, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}

	private static void CompareAppSettingsDefaultEditorApplication(string propertyName,
		AppSettings? sourceIndexItem,
		List<AppSettingsDefaultEditorApplication>? oldKeyValuePairStringStringValue,
		List<AppSettingsDefaultEditorApplication>? newKeyValuePairStringStringValue,
		List<string> differenceList)
	{
		if ( oldKeyValuePairStringStringValue == null ||
		     newKeyValuePairStringStringValue == null ||
		     newKeyValuePairStringStringValue.Count == 0 ||
		     AreListsEqualHelper.AreListsEqual(oldKeyValuePairStringStringValue,
			     newKeyValuePairStringStringValue) )
		{
			return;
		}

		sourceIndexItem?.GetType().GetProperty(propertyName)?.SetValue(sourceIndexItem,
			newKeyValuePairStringStringValue, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}

	private static void CompareMultipleSingleItems(PropertyInfo propertyB,
		PropertyInfo propertyInfoFromA,
		AppSettings sourceIndexItem, object updateObject,
		List<string> differenceList)
	{
		if ( propertyInfoFromA.PropertyType == typeof(bool?) &&
		     propertyB.PropertyType == typeof(bool?) )
		{
			var oldBoolValue = ( bool? ) propertyInfoFromA.GetValue(sourceIndexItem, null);
			var newBoolValue = ( bool? ) propertyB.GetValue(updateObject, null);
			CompareBool(propertyB.Name, sourceIndexItem, oldBoolValue, newBoolValue,
				differenceList);
		}

		if ( propertyB.PropertyType == typeof(string) )
		{
			var oldStringValue = ( string? ) propertyInfoFromA.GetValue(sourceIndexItem, null);
			var newStringValue = ( string? ) propertyB.GetValue(updateObject, null);
			CompareString(propertyB.Name, sourceIndexItem, oldStringValue!, newStringValue!,
				differenceList);
		}

		if ( propertyB.PropertyType == typeof(int) )
		{
			var oldIntValue = ( int ) propertyInfoFromA.GetValue(sourceIndexItem, null)!;
			var newIntValue = ( int ) propertyB.GetValue(updateObject, null)!;
			CompareInt(propertyB.Name, sourceIndexItem, oldIntValue, newIntValue,
				differenceList);
		}

		if ( propertyB.PropertyType == typeof(AppSettings.DatabaseTypeList) )
		{
			var oldListStringValue =
				( AppSettings.DatabaseTypeList? ) propertyInfoFromA.GetValue(sourceIndexItem,
					null);
			var newListStringValue =
				( AppSettings.DatabaseTypeList? ) propertyB.GetValue(updateObject, null);
			CompareDatabaseTypeList(propertyB.Name, sourceIndexItem, oldListStringValue,
				newListStringValue, differenceList);
		}

		if ( propertyB.PropertyType == typeof(CollectionsOpenType.RawJpegMode) )
		{
			var oldRawJpegModeEnumItem =
				( CollectionsOpenType.RawJpegMode? ) propertyInfoFromA.GetValue(sourceIndexItem,
					null);
			var newRawJpegModeEnumItem =
				( CollectionsOpenType.RawJpegMode? ) propertyB.GetValue(updateObject, null);
			CompareCollectionsOpenTypeRawJpegMode(propertyB.Name, sourceIndexItem,
				oldRawJpegModeEnumItem,
				newRawJpegModeEnumItem, differenceList);
		}
	}

	private static void CompareAppSettingsStructureModel(string propertyBName,
		AppSettings sourceIndexItem, AppSettingsStructureModel? oldStructure,
		AppSettingsStructureModel? newStructure, List<string> differenceList)
	{
		if ( oldStructure == null ||
		     newStructure == null ||
		     // compare lists
		     JsonSerializer.Serialize(oldStructure) ==
		     JsonSerializer.Serialize(newStructure) ||
		     // default options
		     JsonSerializer.Serialize(newStructure) ==
		     JsonSerializer.Serialize(new AppSettingsStructureModel()) )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyBName)!.SetValue(sourceIndexItem,
			newStructure, null);
		differenceList.Add(propertyBName.ToLowerInvariant());
	}

	private static void CompareAppSettingsImportTransformationModel(string propertyBName,
		AppSettings sourceIndexItem, AppSettingsImportTransformationModel? oldImportTransformation,
		AppSettingsImportTransformationModel? newImportTransformation, List<string> differenceList)
	{
		if ( oldImportTransformation == null ||
		     newImportTransformation == null ||
		     // compare lists
		     JsonSerializer.Serialize(oldImportTransformation) ==
		     JsonSerializer.Serialize(newImportTransformation) ||
		     // default options
		     JsonSerializer.Serialize(newImportTransformation) ==
		     JsonSerializer.Serialize(new AppSettingsImportTransformationModel()) )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyBName)!.SetValue(sourceIndexItem,
			newImportTransformation, null);
		differenceList.Add(propertyBName.ToLowerInvariant());
	}

	private static void CompareMultipleListDictionary(PropertyInfo propertyB,
		PropertyInfo propertyInfoFromA,
		AppSettings sourceIndexItem, object updateObject, List<string> differenceList)
	{
		if ( propertyB.PropertyType == typeof(List<string>) )
		{
			var oldListStringValue =
				( List<string>? ) propertyInfoFromA.GetValue(sourceIndexItem, null);
			var newListStringValue = ( List<string>? ) propertyB.GetValue(updateObject, null);
			CompareListString(propertyB.Name, sourceIndexItem, oldListStringValue,
				newListStringValue, differenceList);
		}

		if ( propertyB.PropertyType == typeof(List<AppSettingsKeyValue>) )
		{
			var oldKeyValuePairStringStringValue =
				( List<AppSettingsKeyValue>? ) propertyInfoFromA.GetValue(sourceIndexItem, null);
			var newKeyValuePairStringStringValue =
				( List<AppSettingsKeyValue>? ) propertyB.GetValue(updateObject, null);
			CompareKeyValuePairStringString(propertyB.Name, sourceIndexItem,
				oldKeyValuePairStringStringValue!,
				newKeyValuePairStringStringValue!, differenceList);
		}

		if ( propertyB.PropertyType ==
		     typeof(Dictionary<string, List<AppSettingsPublishProfiles>>) )
		{
			var oldListPublishProfilesValue =
				( Dictionary<string, List<AppSettingsPublishProfiles>>? )
				propertyInfoFromA.GetValue(sourceIndexItem, null);
			var newListPublishProfilesValue =
				( Dictionary<string, List<AppSettingsPublishProfiles>>? )
				propertyB.GetValue(updateObject, null);
			CompareListPublishProfiles(propertyB.Name, sourceIndexItem,
				oldListPublishProfilesValue,
				newListPublishProfilesValue, differenceList);
		}

		if ( propertyB.PropertyType == typeof(Dictionary<string, string>) )
		{
			var oldDictionaryValue = ( Dictionary<string, string>? )
				propertyInfoFromA.GetValue(sourceIndexItem, null);
			var newDictionaryValue = ( Dictionary<string, string>? )
				propertyB.GetValue(updateObject, null);
			CompareStringDictionary(propertyB.Name, sourceIndexItem, oldDictionaryValue,
				newDictionaryValue, differenceList);
		}
	}

	private static void CompareStringDictionary(string propertyName,
		AppSettings sourceIndexItem,
		Dictionary<string, string>? oldDictionaryValue,
		Dictionary<string, string>? newDictionaryValue, List<string> differenceList)
	{
		if ( oldDictionaryValue == null || newDictionaryValue?.Count == 0 )
		{
			return;
		}

		if ( JsonSerializer.Serialize(oldDictionaryValue,
			    DefaultJsonSerializer.CamelCase) == JsonSerializer.Serialize(newDictionaryValue,
			    DefaultJsonSerializer.CamelCase) )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyName)
			?.SetValue(sourceIndexItem, newDictionaryValue, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}

	private static void CompareKeyValuePairStringString(string propertyName,
		AppSettings sourceIndexItem,
		List<AppSettingsKeyValue>? oldKeyValuePairStringStringValue,
		List<AppSettingsKeyValue>? newKeyValuePairStringStringValue,
		List<string> differenceList)
	{
		if ( oldKeyValuePairStringStringValue == null ||
		     newKeyValuePairStringStringValue == null ||
		     newKeyValuePairStringStringValue.Count == 0 )
		{
			return;
		}

		if ( oldKeyValuePairStringStringValue.Equals(
			    newKeyValuePairStringStringValue) )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyName)?.SetValue(sourceIndexItem,
			newKeyValuePairStringStringValue, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}

	/// <summary>
	///     Compare DatabaseTypeList type
	/// </summary>
	/// <param name="propertyName">name of property e.g. DatabaseTypeList</param>
	/// <param name="sourceIndexItem">source object</param>
	/// <param name="oldDatabaseTypeList">oldDatabaseTypeList to compare with newDatabaseTypeList</param>
	/// <param name="newDatabaseTypeList">newDatabaseTypeList to compare with oldDatabaseTypeList</param>
	/// <param name="differenceList">list of different values</param>
	internal static void CompareDatabaseTypeList(string propertyName,
		AppSettings sourceIndexItem,
		AppSettings.DatabaseTypeList? oldDatabaseTypeList,
		AppSettings.DatabaseTypeList? newDatabaseTypeList, List<string> differenceList)
	{
		if ( oldDatabaseTypeList == newDatabaseTypeList ||
		     newDatabaseTypeList == new AppSettings().DatabaseType )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyName)
			?.SetValue(sourceIndexItem, newDatabaseTypeList, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}

	/// <summary>
	///     Compare DatabaseTypeList type
	/// </summary>
	/// <param name="propertyName">name of property e.g. DatabaseTypeList</param>
	/// <param name="sourceIndexItem">source object</param>
	/// <param name="oldDatabaseTypeList">oldDatabaseTypeList to compare with newDatabaseTypeList</param>
	/// <param name="newDatabaseTypeList">newDatabaseTypeList to compare with oldDatabaseTypeList</param>
	/// <param name="differenceList">list of different values</param>
	private static void CompareCollectionsOpenTypeRawJpegMode(string propertyName,
		AppSettings sourceIndexItem,
		CollectionsOpenType.RawJpegMode? oldDatabaseTypeList,
		CollectionsOpenType.RawJpegMode? newDatabaseTypeList, List<string> differenceList)
	{
		if ( oldDatabaseTypeList == newDatabaseTypeList ||
		     newDatabaseTypeList == CollectionsOpenType.RawJpegMode.Default )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyName)
			?.SetValue(sourceIndexItem, newDatabaseTypeList, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}


	/// <summary>
	///     Compare List String
	/// </summary>
	/// <param name="propertyName">name of property e.g. Readonly folders</param>
	/// <param name="sourceIndexItem">source object</param>
	/// <param name="oldListStringValue">oldListStringValue to compare with newListStringValue</param>
	/// <param name="newListStringValue">newListStringValue to compare with oldListStringValue</param>
	/// <param name="differenceList">list of different values</param>
	internal static void CompareListString(string propertyName, AppSettings sourceIndexItem,
		List<string>? oldListStringValue, List<string>? newListStringValue,
		List<string> differenceList)
	{
		if ( oldListStringValue == null || newListStringValue?.Count == 0 )
		{
			return;
		}

		if ( JsonSerializer.Serialize(oldListStringValue,
			    DefaultJsonSerializer.CamelCase) == JsonSerializer.Serialize(newListStringValue,
			    DefaultJsonSerializer.CamelCase) )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyName)
			?.SetValue(sourceIndexItem, newListStringValue, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}

	/// <summary>
	///     Compare List AppSettingsPublishProfiles
	/// </summary>
	/// <param name="propertyName">name of property e.g. PublishProfiles</param>
	/// <param name="sourceIndexItem">source object</param>
	/// <param name="oldListPublishValue">oldListPublishValue to compare with newListPublishValue</param>
	/// <param name="newListPublishValue">newListPublishValue to compare with oldListPublishValue</param>
	/// <param name="differenceList">list of different values</param>
	internal static void CompareListPublishProfiles(string propertyName,
		AppSettings sourceIndexItem,
		Dictionary<string, List<AppSettingsPublishProfiles>>? oldListPublishValue,
		Dictionary<string, List<AppSettingsPublishProfiles>>? newListPublishValue,
		List<string> differenceList)
	{
		if ( oldListPublishValue == null || newListPublishValue?.Count == 0 )
		{
			return;
		}

		if ( oldListPublishValue.Equals(newListPublishValue) )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyName)
			?.SetValue(sourceIndexItem, newListPublishValue, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}


	/// <summary>
	///     Compare bool type
	/// </summary>
	/// <param name="propertyName">name of property e.g. AddSwagger</param>
	/// <param name="sourceIndexItem">source object</param>
	/// <param name="oldBoolValue">oldBoolValue to compare with newBoolValue</param>
	/// <param name="newBoolValue">oldBoolValue to compare with newBoolValue</param>
	/// <param name="differenceList">list of different values</param>
	internal static void CompareBool(string propertyName, AppSettings sourceIndexItem,
		bool? oldBoolValue,
		bool? newBoolValue, List<string> differenceList)
	{
		if ( newBoolValue == null )
		{
			return;
		}

		if ( oldBoolValue == newBoolValue )
		{
			return;
		}

		sourceIndexItem.GetType().GetProperty(propertyName)
			?.SetValue(sourceIndexItem, newBoolValue, null);
		differenceList.Add(propertyName.ToLowerInvariant());
	}

	/// <summary>
	///     Compare string type
	/// </summary>
	/// <param name="propertyName">name of property e.g. Tags</param>
	/// <param name="sourceIndexItem">source object</param>
	/// <param name="oldStringValue">oldStringValue to compare with newStringValue</param>
	/// <param name="newStringValue">oldStringValue to compare with newStringValue</param>
	/// <param name="differenceList">list of different values</param>
	internal static void CompareString(string propertyName, AppSettings sourceIndexItem,
		string oldStringValue, string newStringValue,
		List<string> differenceList)
	{
		var newAppSettings = new AppSettings();

		var defaultValue = GetPropertyValue(newAppSettings, propertyName) as string;
		if ( newStringValue == defaultValue )
		{
			return;
		}

		if ( oldStringValue == newStringValue ||
		     string.IsNullOrEmpty(newStringValue) )
		{
			return;
		}

		var propertyObject = sourceIndexItem.GetType().GetProperty(propertyName);

		propertyObject?.SetValue(sourceIndexItem, newStringValue, null);

		differenceList.Add(propertyName.ToLowerInvariant());
	}


	/// <summary>
	///     Compare string type
	/// </summary>
	/// <param name="propertyName">name of property e.g. Tags</param>
	/// <param name="sourceIndexItem">source object</param>
	/// <param name="oldStringValue">oldStringValue to compare with newStringValue</param>
	/// <param name="newStringValue">oldStringValue to compare with newStringValue</param>
	/// <param name="differenceList">list of different values</param>
	internal static void CompareInt(string propertyName, AppSettings sourceIndexItem,
		int oldStringValue, int newStringValue,
		List<string> differenceList)
	{
		var newAppSettings = new AppSettings();

		var defaultValue = GetPropertyValue(newAppSettings, propertyName) as int?;
		if ( newStringValue == defaultValue )
		{
			return;
		}

		if ( oldStringValue == newStringValue || newStringValue == 0 )
		{
			return;
		}

		var propertyObject = sourceIndexItem.GetType().GetProperty(propertyName);

		propertyObject?.SetValue(sourceIndexItem, newStringValue, null);

		differenceList.Add(propertyName.ToLowerInvariant());
	}

	/// <summary>
	///     @see: https://stackoverflow.com/a/5508068
	/// </summary>
	/// <param name="car"></param>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	private static object? GetPropertyValue(object car, string propertyName)
	{
		return Array.Find(car.GetType().GetProperties(), pi => pi.Name == propertyName)?
			.GetValue(car, null);
	}
}
