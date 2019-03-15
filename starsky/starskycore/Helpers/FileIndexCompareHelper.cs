﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using starskycore.Models;

namespace starskycore.Helpers
{
    public static class FileIndexCompareHelper
    {
        /// <summary>
        /// Compare a fileindex item and update items if there are changed in the updateObject
        /// append => (propertyName == "Tags" add it with comma space or with single space)
        /// </summary>
        /// <param name="sourceIndexItem">the source object</param>
        /// <param name="updateObject">the item with changed values</param>
        /// <param name="append">when the type is string add it to existing values </param>
        /// <returns>list of changed types</returns>
        public static List<string> Compare(FileIndexItem sourceIndexItem, FileIndexItem updateObject = null, bool append = false)
        {
            if(updateObject == null) updateObject = new FileIndexItem();
            PropertyInfo[] propertiesA = sourceIndexItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] propertiesB = updateObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var differenceList = new List<string>();
	        int count = propertiesA.Length;
            for (int i = 0; i < count; i++)
            {
                if ((!propertiesA[i].CanRead) || (!propertiesB[i].CanRead)) continue;
                
                if (propertiesA[i].PropertyType == typeof(string))
                {
                    var oldStringValue = (string)propertiesA[i].GetValue(sourceIndexItem, null);
                    var newStringValue = (string)propertiesB[i].GetValue(updateObject, null);
                    CompareString(propertiesB[i].Name, sourceIndexItem, oldStringValue, newStringValue, differenceList, append);
                }

                if (propertiesA [i].PropertyType == typeof(bool))
                {
                    var oldBoolValue = (bool)propertiesA [i].GetValue(sourceIndexItem, null);
                    var newBoolValue = (bool)propertiesB [i].GetValue(updateObject, null);
                    CompareBool(propertiesB[i].Name, sourceIndexItem, oldBoolValue, newBoolValue, differenceList);
                }
                    
                if (propertiesA [i].PropertyType == typeof(FileIndexItem.Color))
                {
                    var oldColorValue = (FileIndexItem.Color)propertiesA [i].GetValue(sourceIndexItem, null);
                    var newColorValue = (FileIndexItem.Color)propertiesB [i].GetValue(updateObject, null);
                    CompareColor(propertiesB[i].Name, sourceIndexItem, oldColorValue, newColorValue, differenceList);
                }

                if (propertiesA[i].PropertyType == typeof(DateTime))
                {
                    var oldDateValue = (DateTime)propertiesA [i].GetValue(sourceIndexItem, null);
                    var newDateValue = (DateTime)propertiesB [i].GetValue(updateObject, null);
                    CompareDateTime(propertiesB[i].Name, sourceIndexItem, oldDateValue, newDateValue, differenceList); 
                }
                
                if (propertiesA[i].PropertyType == typeof(FileIndexItem.Rotation))
                {
                    var oldRotationValue = (FileIndexItem.Rotation)propertiesA [i].GetValue(sourceIndexItem, null);
                    var newRotationValue = (FileIndexItem.Rotation)propertiesB [i].GetValue(updateObject, null);
                    CompareRotation(propertiesB[i].Name, sourceIndexItem, oldRotationValue, newRotationValue, differenceList); 
                }
            }
            return differenceList;
        }

	    /// <summary>
	    /// Set values by string name. fieldContent must by the right type
	    /// wrong types are ignored by default
	    /// </summary>
	    /// <param name="sourceIndexItem">fileIndexItem to add to</param>
	    /// <param name="fieldName">name e.g. tags or description</param>
	    /// <param name="fieldContent">the content, type must match exact</param>
	    /// <returns>fileIndexItem</returns>
	    public static FileIndexItem Set(FileIndexItem sourceIndexItem, string fieldName, object fieldContent)
	    {
		    if(sourceIndexItem == null) sourceIndexItem = new FileIndexItem();
		    if ( !CheckIfPropertyExist(fieldName) ) return sourceIndexItem;
		    
		    // Compare input types, fieldType(object=string) fileIndexType(FileIndexItem.field=string)
		    // wrong types are ignored by default
		    var fieldType = fieldContent.GetType();
		    var fileIndexType = sourceIndexItem.GetType().GetProperty(fieldName).PropertyType;
		    if ( fileIndexType == fieldType )
		    {
			    sourceIndexItem.GetType().GetProperty(fieldName).SetValue(sourceIndexItem, fieldContent, null);
		    }
		    return sourceIndexItem;
	    }
	    
	    /// <summary>
	    /// Get property in FileIndexItem
	    /// </summary>
	    /// <param name="fieldName">name e.g. Tags</param>
	    /// <returns>object type is defined in fileindexitem</returns>
	    public static object Get(FileIndexItem sourceIndexItem, string fieldName)
	    {
		    if ( CheckIfPropertyExist(fieldName) && sourceIndexItem != null)
		    {
			    return sourceIndexItem.GetType().GetProperty(fieldName).GetValue(sourceIndexItem, null);
		    }
		    return null;
	    }
	    
	    
	    /// <summary>
	    /// Check if property exist in FileIndexItem
	    /// </summary>
	    /// <param name="fieldName">name e.g. Tags</param>
	    /// <returns>bool, true=exist</returns>
	    public static bool CheckIfPropertyExist(string fieldName)
	    {
		    PropertyInfo[] propertiesA = new FileIndexItem().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
		    return propertiesA.Any(p => p.Name == fieldName);
	    }
	    
//	    /// <summary>
//	    /// Get the exact mating property name, so Tags instead of TAgs
//	    /// </summary>
//	    /// <param name="fieldName">name e.g. Tags</param>
//	    /// <returns>bool, true=exist</returns>
//	    public static string GetExactPropertyName(string fieldName)
//	    {
//		    if ( !CheckIfPropertyExist(fieldName) ) return string.Empty;
//
//	    }
//		    
//		    PropertyInfo[] propertiesA = new FileIndexItem().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
//		    return propertiesA.Any(p => p.Name == fieldName);
//	    }

	    /// <summary>
	    /// Update compared values
	    /// </summary>
	    /// <param name="sourceIndexItem">the source object</param>
	    /// <param name="updateObject">the item with changed values</param>
	    /// <param name="differenceList">typeName of item</param>
	    /// <returns></returns>
	    public static FileIndexItem SetCompare(FileIndexItem sourceIndexItem, FileIndexItem updateObject, List<string> differenceList)
	    {
		    if(updateObject == null) updateObject = new FileIndexItem();
		    PropertyInfo[] propertiesA = sourceIndexItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
		    PropertyInfo[] propertiesB = updateObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		    int count = propertiesA.Length;
		    for ( int i = 0; i < count; i++ )
		    {
			    if ( ( !propertiesA[i].CanRead ) || ( !propertiesB[i].CanRead ) ) continue;
			    if(!differenceList.Contains(propertiesA[i].Name)) continue;
			    
			    var newRotationValue = propertiesB [i].GetValue(updateObject, null);
			    
			    sourceIndexItem.GetType().GetProperty(propertiesA[i].Name).SetValue(sourceIndexItem, newRotationValue, null);
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
	    private static void CompareRotation(string propertyName, FileIndexItem sourceIndexItem, FileIndexItem.Rotation oldRotationValue,
            FileIndexItem.Rotation newRotationValue, List<string> differenceList)
        {
            if (oldRotationValue == newRotationValue || newRotationValue == FileIndexItem.Rotation.DoNotChange) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newRotationValue, null);
            differenceList.Add(propertyName);
        }

	    /// <summary>
	    /// Compare DateTime type 
	    /// </summary>
	    /// <param name="propertyName">name of property e.g. DateTime</param>
	    /// <param name="sourceIndexItem">source object</param>
	    /// <param name="oldDateValue">oldDateValue to compare with newDateValue</param>
	    /// <param name="newDateValue">oldDateValue to compare with newDateValue</param>
	    /// <param name="differenceList">list of different values</param>
        private static void CompareDateTime(string propertyName, FileIndexItem sourceIndexItem, DateTime oldDateValue, DateTime newDateValue, List<string> differenceList)
        {
            // Dont allow to overwrite with default year 0001
            if (oldDateValue == newDateValue || newDateValue.Year < 2) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newDateValue, null);
            differenceList.Add(propertyName);
        }
        
	    /// <summary>
	    /// Compare ColorClass type 
	    /// </summary>
	    /// <param name="propertyName">name of property e.g. ColorClass</param>
	    /// <param name="sourceIndexItem">source object</param>
	    /// <param name="oldColorValue">oldColorValue to compare with newColorValue</param>
	    /// <param name="newColorValue">oldColorValue to compare with newColorValue</param>
	    /// <param name="differenceList">list of different values</param>
        private static void CompareColor(string propertyName, FileIndexItem sourceIndexItem, FileIndexItem.Color oldColorValue, FileIndexItem.Color newColorValue, List<string> differenceList)
        {
            if (oldColorValue == newColorValue || newColorValue == FileIndexItem.Color.DoNotChange) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newColorValue, null);
            differenceList.Add(propertyName);
        }
        
	    
	    /// <summary>
	    /// Compare bool type 
	    /// </summary>
	    /// <param name="propertyName">name of property e.g. IsDirectory</param>
	    /// <param name="sourceIndexItem">source object</param>
	    /// <param name="oldBoolValue">oldBoolValue to compare with newBoolValue</param>
	    /// <param name="newBoolValue">oldBoolValue to compare with newBoolValue</param>
	    /// <param name="differenceList">list of different values</param>
        private static void CompareBool(string propertyName, FileIndexItem sourceIndexItem, bool oldBoolValue, bool newBoolValue, List<string> differenceList)
        {
            if (oldBoolValue == newBoolValue) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newBoolValue, null);
            differenceList.Add(propertyName);
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
        private static void CompareString(string propertyName, FileIndexItem sourceIndexItem, string oldStringValue, string newStringValue, List<string> differenceList, bool append)
        {
            if (oldStringValue == newStringValue ||
                (string.IsNullOrEmpty(newStringValue) && newStringValue != "/")) return;
            
            if (propertyName == "FileName") return;
         
            var propertyObject = sourceIndexItem.GetType().GetProperty(propertyName);
                        
            if (!append)
            {
                propertyObject.SetValue(sourceIndexItem, newStringValue, null);
            }
            else if (propertyName == "Tags")
            {
                propertyObject.SetValue(sourceIndexItem, oldStringValue + ", " + newStringValue,null);
            }
            else
            {
                propertyObject.SetValue(sourceIndexItem,oldStringValue + " " + newStringValue,null);
            }
            
            differenceList.Add(propertyName);
        }

    }
}