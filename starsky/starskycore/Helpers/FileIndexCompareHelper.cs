using System;
using System.Collections.Generic;
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

            int count = propertiesA.Length;
            var differenceList = new List<string>();
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

	    private static void CompareRotation(string propertyName, FileIndexItem sourceIndexItem, FileIndexItem.Rotation oldRotationValue,
            FileIndexItem.Rotation newRotationValue, List<string> differenceList)
        {
            if (oldRotationValue == newRotationValue || newRotationValue == FileIndexItem.Rotation.DoNotChange) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newRotationValue, null);
            differenceList.Add(propertyName);
        }

        private static void CompareDateTime(string propertyName, FileIndexItem sourceIndexItem, DateTime oldDateValue, DateTime newDateValue, List<string> differenceList)
        {
            // Dont allow to overwrite with default year 0001
            if (oldDateValue == newDateValue || newDateValue.Year < 2) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newDateValue, null);
            differenceList.Add(propertyName);
        }
        
        private static void CompareColor(string propertyName, FileIndexItem sourceIndexItem, FileIndexItem.Color oldColorValue, FileIndexItem.Color newColorValue, List<string> differenceList)
        {
            if (oldColorValue == newColorValue || newColorValue == FileIndexItem.Color.DoNotChange) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newColorValue, null);
            differenceList.Add(propertyName);
        }
        
        private static void CompareBool(string propertyName, FileIndexItem sourceIndexItem, bool oldBoolValue, bool newBoolValue, List<string> differenceList)
        {
            if (oldBoolValue == newBoolValue) return;
            sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newBoolValue, null);
            differenceList.Add(propertyName);
        }

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