using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Helpers
{
	public static class FileIndexCompareHelper
	{
		/// <summary>
		/// Compare a fileIndex item and update items if there are changed in the updateObject
		/// append => (propertyName == "Tags" add it with comma space or with single space)
		/// </summary>
		/// <param name="sourceIndexItem">the source object</param>
		/// <param name="updateObject">the item with changed values</param>
		/// <param name="append">when the type is string add it to existing values </param>
		/// <returns>list of changed types</returns>
		[SuppressMessage("Usage",
			"S6602: Find method should be used instead of the FirstOrDefault extension method.")]
		public static List<string> Compare(FileIndexItem sourceIndexItem,
			FileIndexItem? updateObject = null, bool append = false)
		{
			if ( updateObject == null ) updateObject = new FileIndexItem();

			var differenceList = new List<string>();
			PropertyInfo[] propertiesA = sourceIndexItem.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			PropertyInfo[] propertiesB = updateObject.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			for ( int i = 0; i < propertiesA.Length; i++ )
			{
				var propertyA = propertiesA[i];
				var propertyB = propertiesB.FirstOrDefault(p => p.Name == propertyA.Name);

				if ( PropertyCanRead(propertyA, propertyB) )
					continue;

				Type propertyType = propertyA.PropertyType;
				var oldValue = propertyA.GetValue(sourceIndexItem, null);
				var newValue = propertyB!.GetValue(updateObject, null);

				if ( propertyType == typeof(string) )
				{
					CompareString(propertyB.Name, sourceIndexItem, ( string? )oldValue,
						( string? )newValue, differenceList, append);
				}
				else if ( propertyType == typeof(bool?) )
				{
					CompareNullableBool(propertyB.Name, sourceIndexItem, ( bool? )oldValue,
						( bool? )newValue, differenceList);
				}
				else if ( propertyType == typeof(ColorClassParser.Color) )
				{
					CompareColor(propertyB.Name, sourceIndexItem,
						( ColorClassParser.Color? )oldValue,
						( ColorClassParser.Color? )newValue, differenceList);
				}
				else if ( propertyType == typeof(DateTime) )
				{
					CompareDateTime(propertyB.Name, sourceIndexItem, ( DateTime? )oldValue,
						( DateTime? )newValue, differenceList);
				}
				else if ( propertyType == typeof(FileIndexItem.Rotation) )
				{
					CompareRotation(propertyB.Name, sourceIndexItem,
						( FileIndexItem.Rotation? )oldValue,
						( FileIndexItem.Rotation? )newValue, differenceList);
				}
				else if ( propertyType == typeof(ImageStabilisationType) )
				{
					CompareImageStabilisationType(propertyB.Name, sourceIndexItem,
						( ImageStabilisationType? )oldValue,
						( ImageStabilisationType? )newValue, differenceList);
				}
				else if ( propertyType == typeof(double) )
				{
					CompareDouble(propertyB.Name, sourceIndexItem, ( double? )oldValue,
						( double? )( newValue ), differenceList);
				}
				else if ( propertyType == typeof(ushort) )
				{
					CompareUshort(propertyB.Name, sourceIndexItem, ( ushort? )oldValue,
						( ushort? )newValue, differenceList);
				}
				else if ( propertyType == typeof(List<string>) )
				{
					CompareListString(propertyB.Name, sourceIndexItem, ( List<string>? )oldValue,
						( List<string>? )newValue, differenceList);
				}
				else if ( propertyType == typeof(ExtensionRolesHelper.ImageFormat) )
				{
					CompareImageFormat(propertyB.Name, sourceIndexItem,
						( ExtensionRolesHelper.ImageFormat? )oldValue,
						( ExtensionRolesHelper.ImageFormat? )newValue, differenceList);
				}
			}

			// Remove unnecessary properties
			differenceList.Remove(nameof(FileIndexItem.LastEdited).ToLowerInvariant());
			differenceList.Remove(nameof(FileIndexItem.LastChanged).ToLowerInvariant());

			return differenceList;
		}

		private static bool PropertyCanRead(PropertyInfo propertyA, PropertyInfo? propertyB)
		{
			return propertyB == null || !propertyA.CanRead ||
			       !propertyB.CanRead;
		}

		/// <summary>
		/// Set values by string name. fieldContent must by the right type
		/// wrong types are ignored by default
		/// </summary>
		/// <param name="sourceIndexItem">fileIndexItem to add to</param>
		/// <param name="fieldName">name e.g. tags or description</param>
		/// <param name="fieldContent">the content, type must match exact</param>
		/// <returns>fileIndexItem</returns>
		public static FileIndexItem Set(FileIndexItem? sourceIndexItem, string fieldName,
			object fieldContent)
		{
			sourceIndexItem ??= new FileIndexItem();
			if ( !CheckIfPropertyExist(fieldName) )
			{
				return sourceIndexItem;
			}

			// Compare input types, fieldType(object=string) fileIndexType(FileIndexItem.field=string)
			// wrong types are ignored by default

			var propertiesA = new FileIndexItem().GetType().GetProperties(
				BindingFlags.Public | BindingFlags.Instance);
			var property = Array.Find(propertiesA, p =>
				string.Equals(p.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));

			var fieldType = fieldContent.GetType();
			var fileIndexType = property?.PropertyType;
			if ( fileIndexType == fieldType )
			{
				property?.SetValue(sourceIndexItem, fieldContent, null);
			}

			return sourceIndexItem;
		}

		/// <summary>
		/// Get property in FileIndexItem
		/// </summary>
		/// <param name="sourceIndexItem">where from</param>
		/// <param name="fieldName">name e.g. Tags</param>
		/// <returns>object type is defined in fileIndexItem</returns>
		public static object? Get(FileIndexItem? sourceIndexItem, string fieldName)
		{
			if ( CheckIfPropertyExist(fieldName) && sourceIndexItem != null )
			{
				var propertiesA = new FileIndexItem().GetType()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				return Array.Find(propertiesA, p =>
						string.Equals(p.Name, fieldName,
							StringComparison.InvariantCultureIgnoreCase))?
					.GetValue(sourceIndexItem, null);
			}

			return null;
		}

		/// <summary>
		/// Check if property exist in FileIndexItem
		/// </summary>
		/// <param name="fieldName">name e.g. Tags</param>
		/// <returns>bool, true=exist</returns>
		[SuppressMessage("Usage", "S6605: Collection-specific Exists " +
		                          "method should be used instead of the Any extension.")]
		public static bool CheckIfPropertyExist(string fieldName)
		{
			PropertyInfo[] propertiesA = new FileIndexItem().GetType().GetProperties(
				BindingFlags.Public | BindingFlags.Instance);
			return propertiesA.Any(p => string.Equals(p.Name,
				fieldName, StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Update compared values
		/// </summary>
		/// <param name="sourceIndexItem">the source object</param>
		/// <param name="updateObject">the item with changed values</param>
		/// <param name="differenceList">typeName of item</param>
		/// <returns></returns>
		public static FileIndexItem SetCompare(FileIndexItem sourceIndexItem,
			FileIndexItem? updateObject, List<string> differenceList)
		{
			updateObject ??= new FileIndexItem();
			var propertiesA = sourceIndexItem.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var propertiesB = updateObject.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			int count = propertiesA.Length;
			for ( int i = 0; i < count; i++ )
			{
				if ( ( !propertiesA[i].CanRead ) || ( !propertiesB[i].CanRead ) ) continue;
				if ( !differenceList.Contains(propertiesA[i].Name) ) continue;

				var newRotationValue = propertiesB[i].GetValue(updateObject, null);

				sourceIndexItem.GetType().GetProperty(propertiesA[i].Name)!.SetValue(
					sourceIndexItem, newRotationValue, null);
			}

			return sourceIndexItem;
		}

		/// <summary>
		/// Compare rotation type
		/// </summary>
		/// <param name="propertyName">name of property e.g. Rotation</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldRotationValue">oldRotationValue to compare with newRotationValue</param>
		/// <param name="newRotationValue">oldRotationValue to compare with newRotationValue</param>
		/// <param name="differenceList">list of different values</param>
		internal static void CompareRotation(string propertyName, FileIndexItem sourceIndexItem,
			FileIndexItem.Rotation? oldRotationValue, FileIndexItem.Rotation? newRotationValue,
			List<string> differenceList)
		{
			if ( oldRotationValue == newRotationValue || newRotationValue == null ||
			     newRotationValue == FileIndexItem.Rotation.DoNotChange ) return;
			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newRotationValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare double type 
		/// </summary>
		/// <param name="propertyName">name of property e.g. aperture</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldDoubleValue">oldDoubleValue to compare with newDoubleValue</param>
		/// <param name="newDoubleValue">oldDoubleValue to compare with newDoubleValue</param>
		/// <param name="differenceList">list of different values</param>
		internal static void CompareDouble(string propertyName, FileIndexItem sourceIndexItem,
			double? oldDoubleValue, double? newDoubleValue, List<string> differenceList)
		{
			const double tolerance = 0.000001d;
			// Dont allow to overwrite with default 0 value
			if ( oldDoubleValue == null ||
			     newDoubleValue == null ||
			     Math.Abs(oldDoubleValue.Value - newDoubleValue.Value) < tolerance ||
			     Math.Abs(newDoubleValue.Value) < tolerance )
			{
				return;
			}

			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newDoubleValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare ushort type 
		/// </summary>
		/// <param name="propertyName">name of property e.g. isoSpeed</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldUshortValue">oldUshortValue to compare with newUshortValue</param>
		/// <param name="newUshortValue">oldUshortValue to compare with newUshortValue</param>
		/// <param name="differenceList">list of different values</param>
		internal static void CompareUshort(string propertyName, FileIndexItem sourceIndexItem,
			ushort? oldUshortValue, ushort? newUshortValue, List<string> differenceList)
		{
			// Dont allow to overwrite with default 0 value
			if ( oldUshortValue == newUshortValue || newUshortValue == null ||
			     newUshortValue == 0 ) return;
			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newUshortValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare imageFormat type
		/// </summary>
		/// <param name="propertyName">name of property e.g. ImageFormat</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldImageFormatValue">two values to compare with</param>
		/// <param name="newImageFormatValue">two values to compare with</param>
		/// <param name="differenceList">list of differences</param>
		internal static void CompareImageFormat(string propertyName, FileIndexItem sourceIndexItem,
			ExtensionRolesHelper.ImageFormat? oldImageFormatValue,
			ExtensionRolesHelper.ImageFormat? newImageFormatValue, List<string> differenceList)
		{
			if ( oldImageFormatValue == newImageFormatValue || newImageFormatValue == null ||
			     newImageFormatValue == ExtensionRolesHelper.ImageFormat.unknown ) return;
			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newImageFormatValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare image stab value
		/// </summary>
		/// <param name="propertyName">name of property e.g. image stab</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldImageStabValue">two values to compare with</param>
		/// <param name="newImageStabValue">two values to compare with</param>
		/// <param name="differenceList">list of differences</param>
		private static void CompareImageStabilisationType(string propertyName,
			FileIndexItem sourceIndexItem,
			ImageStabilisationType? oldImageStabValue, ImageStabilisationType? newImageStabValue,
			List<string> differenceList)
		{
			if ( oldImageStabValue == newImageStabValue || newImageStabValue == null ||
			     newImageStabValue == ImageStabilisationType.Unknown ) return;
			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newImageStabValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare DateTime type 
		/// </summary>
		/// <param name="propertyName">name of property e.g. DateTime</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldDateValue">oldDateValue to compare with newDateValue</param>
		/// <param name="newDateValue">oldDateValue to compare with newDateValue</param>
		/// <param name="differenceList">list of different values</param>
		internal static void CompareDateTime(string propertyName, FileIndexItem sourceIndexItem,
			DateTime? oldDateValue, DateTime? newDateValue, List<string> differenceList)
		{
			// Dont allow to overwrite with default year 0001
			if ( oldDateValue == newDateValue || newDateValue == null ||
			     newDateValue.Value.Year < 2 ) return;
			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newDateValue.Value, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare ColorClass type 
		/// </summary>
		/// <param name="propertyName">name of property e.g. ColorClass</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldColorValue">oldColorValue to compare with newColorValue</param>
		/// <param name="newColorValue">oldColorValue to compare with newColorValue</param>
		/// <param name="differenceList">list of different values</param>
		internal static void CompareColor(string propertyName, FileIndexItem sourceIndexItem,
			ColorClassParser.Color? oldColorValue,
			ColorClassParser.Color? newColorValue, List<string> differenceList)
		{
			if ( oldColorValue == newColorValue ||
			     newColorValue == ColorClassParser.Color.DoNotChange ||
			     newColorValue == null ) return;
			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newColorValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}


		/// <summary>
		/// Compare List String
		/// </summary>
		/// <param name="propertyName">name of property e.g. CollectionPaths</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldListStringValue">oldListStringValue to compare with newListStringValue</param>
		/// <param name="newListStringValue">newListStringValue to compare with oldListStringValue</param>
		/// <param name="differenceList">list of different values</param>
		internal static void CompareListString(string propertyName, FileIndexItem sourceIndexItem,
			List<string>? oldListStringValue, List<string>? newListStringValue,
			List<string> differenceList)
		{
			if ( oldListStringValue == null || newListStringValue?.Count == 0 ) return;
			if ( oldListStringValue.Equals(newListStringValue) ) return;

			sourceIndexItem.GetType().GetProperty(propertyName)
				?.SetValue(sourceIndexItem, newListStringValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare Nullable bool type 
		/// </summary>
		/// <param name="propertyName">name of property e.g. IsDirectory</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldBoolValue">oldBoolValue to compare with newBoolValue</param>
		/// <param name="newBoolValue">oldBoolValue to compare with newBoolValue</param>
		/// <param name="differenceList">list of different values</param>
		internal static void CompareNullableBool(string propertyName, FileIndexItem sourceIndexItem,
			bool? oldBoolValue, bool? newBoolValue, List<string> differenceList)
		{
			if ( newBoolValue == null || oldBoolValue == newBoolValue ) return;
			var property = sourceIndexItem.GetType().GetProperty(propertyName);
			if ( property == null ) return;
			property.SetValue(sourceIndexItem, newBoolValue, null);
			differenceList.Add(propertyName.ToLowerInvariant());
		}

		/// <summary>
		/// Compare string type 
		/// </summary>
		/// <param name="propertyName">name of property e.g. Tags</param>
		/// <param name="sourceIndexItem">source object</param>
		/// <param name="oldStringValue">oldStringValue to compare with newStringValue</param>
		/// <param name="newStringValue">oldStringValue to compare with newStringValue</param>
		/// <param name="differenceList">list of different values</param>
		/// <param name="append">to add after list (if tags)</param>
		internal static void CompareString(string propertyName, FileIndexItem sourceIndexItem,
			string? oldStringValue, string? newStringValue,
			List<string> differenceList, bool append)
		{
			// ignore capitals
			if ( string.IsNullOrEmpty(newStringValue) ||
			     string.Equals(oldStringValue, newStringValue,
				     StringComparison.InvariantCultureIgnoreCase) ) return;

			if ( propertyName is nameof(FileIndexItem.FileName) or nameof(FileIndexItem.FilePath) )
			{
				return;
			}

			var propertyObject = sourceIndexItem.GetType().GetProperty(propertyName);

			if ( propertyObject == null || !propertyObject.CanWrite )
			{
				return;
			}

			if ( !append )
			{
				newStringValue = StringHelper.AsciiNullReplacer(newStringValue);
				propertyObject.SetValue(sourceIndexItem, newStringValue, null);
			}
			// only for appending tags: ==>
			else if ( propertyName == nameof(FileIndexItem.Tags) )
			{
				propertyObject.SetValue(sourceIndexItem, oldStringValue + ", " + newStringValue,
					null);
			}
			else
			{
				propertyObject.SetValue(sourceIndexItem, oldStringValue + " " + newStringValue,
					null);
			}

			differenceList.Add(propertyName.ToLowerInvariant());
		}
	}
}
