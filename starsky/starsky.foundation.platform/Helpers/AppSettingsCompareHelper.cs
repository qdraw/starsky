using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Helpers
{
    public static class AppSettingsCompareHelper
    {

	    /// <summary>
        /// Compare a fileIndex item and update items if there are changed in the updateObject
        /// append => (propertyName == "Tags" add it with comma space or with single space)
        /// </summary>
        /// <param name="sourceIndexItem">the source object</param>
        /// <param name="updateObject">the item with changed values</param>
        /// <returns>list of changed types</returns>
        public static List<string> Compare(AppSettings sourceIndexItem, AppSettings updateObject = null)
        {
            if(updateObject == null) updateObject = new AppSettings();
            PropertyInfo[] propertiesA = sourceIndexItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] propertiesB = updateObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var differenceList = new List<string>();
            for (var i = 0; i < propertiesA.Length; i++)
            {
                if ((!propertiesA[i].CanRead) || (!propertiesB[i].CanRead)) continue;

                if (propertiesA [i].PropertyType == typeof(bool))
                {
                    var oldBoolValue = (bool)propertiesA [i].GetValue(sourceIndexItem, null);
                    var newBoolValue = (bool)propertiesB [i].GetValue(updateObject, null);
                    CompareBool(propertiesB[i].Name, sourceIndexItem, oldBoolValue, newBoolValue, differenceList);
                }
                
                if (propertiesA[i].PropertyType == typeof(string))
                {
	                var oldStringValue = (string)propertiesA[i].GetValue(sourceIndexItem, null);
	                var newStringValue = (string)propertiesB[i].GetValue(updateObject, null);
	                CompareString(propertiesB[i].Name, sourceIndexItem, oldStringValue, newStringValue, differenceList);
                }
                
	            if ( propertiesA[i].PropertyType == typeof(List<string>) )
	            {
		            var oldListStringValue = ( List<string> ) propertiesA[i].GetValue(sourceIndexItem, null);
		            var newListStringValue = ( List<string> ) propertiesB[i].GetValue(updateObject, null);
		            CompareListString(propertiesB[i].Name, sourceIndexItem, oldListStringValue,
			            newListStringValue, differenceList);
	            }
	            
	            if ( propertiesA[i].PropertyType == typeof(AppSettings.DatabaseTypeList) )
	            {
		            var oldListStringValue = ( AppSettings.DatabaseTypeList ) propertiesA[i].GetValue(sourceIndexItem, null);
		            var newListStringValue = ( AppSettings.DatabaseTypeList ) propertiesB[i].GetValue(updateObject, null);
		            CompareDatabaseTypeList(propertiesB[i].Name, sourceIndexItem, oldListStringValue,
			            newListStringValue, differenceList);
	            }
	            
	            if ( propertiesA[i].PropertyType == typeof(List<AppSettingsPublishProfiles>) )
	            {
		            var oldListPublishProfilesValue = ( List<AppSettingsPublishProfiles> ) propertiesA[i].GetValue(sourceIndexItem, null);
		            var newListPublishProfilesValue = ( List<AppSettingsPublishProfiles> ) propertiesB[i].GetValue(updateObject, null);
		            CompareListPublishProfiles(propertiesB[i].Name, sourceIndexItem, oldListPublishProfilesValue,
			            newListPublishProfilesValue, differenceList);
	            }
            }
	        
            return differenceList;
        }
        
        // /// <summary>
        // /// Set values by string name. fieldContent must by the right type
        // /// wrong types are ignored by default
        // /// </summary>
        // /// <param name="sourceIndexItem">fileIndexItem to add to</param>
        // /// <param name="fieldName">name e.g. tags or description</param>
        // /// <param name="fieldContent">the content, type must match exact</param>
        // /// <returns>fileIndexItem</returns>
        // public static AppSettings Set(AppSettings sourceIndexItem, string fieldName, object fieldContent)
        // {
	       //  if(sourceIndexItem == null) sourceIndexItem = new AppSettings();
	       //  if ( !CheckIfPropertyExist(fieldName) ) return sourceIndexItem;
		      //
	       //  // Compare input types, fieldType(object=string) fileIndexType(FileIndexItem.field=string)
	       //  // wrong types are ignored by default
		      //
	       //  PropertyInfo[] propertiesA = new AppSettings().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
	       //  var property = propertiesA.FirstOrDefault(p =>
		      //   string.Equals(p.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));
		      //
	       //  var fieldType = fieldContent.GetType();
	       //  var fileIndexType = property.PropertyType;
	       //  if ( fileIndexType == fieldType )
	       //  {
		      //   property.SetValue(sourceIndexItem, fieldContent, null);
	       //  }
	       //  return sourceIndexItem;
        // }
        //
        // /// <summary>
        // /// Check if property exist in AppSettings
        // /// </summary>
        // /// <param name="fieldName">name e.g. Tags</param>
        // /// <returns>bool, true=exist</returns>
        // private static bool CheckIfPropertyExist(string fieldName)
        // {
	       //  PropertyInfo[] propertiesA = new AppSettings().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
	       //  return propertiesA.Any(p => string.Equals(p.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));
        // }
        //
        
        
        /// <summary>
        /// Compare DatabaseTypeList type 
        /// </summary>
        /// <param name="propertyName">name of property e.g. DatabaseTypeList</param>
        /// <param name="sourceIndexItem">source object</param>
        /// <param name="oldDatabaseTypeList">oldDatabaseTypeList to compare with newDatabaseTypeList</param>
        /// <param name="newDatabaseTypeList">newDatabaseTypeList to compare with oldDatabaseTypeList</param>
        /// <param name="differenceList">list of different values</param>
        private static void CompareDatabaseTypeList(string propertyName, AppSettings sourceIndexItem, AppSettings.DatabaseTypeList oldDatabaseTypeList, 
	        AppSettings.DatabaseTypeList newDatabaseTypeList, List<string> differenceList)
        {
	        if (oldDatabaseTypeList == newDatabaseTypeList || newDatabaseTypeList == new AppSettings().DatabaseType) return;
	        sourceIndexItem.GetType().GetProperty(propertyName).SetValue(sourceIndexItem, newDatabaseTypeList, null);
	        differenceList.Add(propertyName.ToLowerInvariant());
        }
        
	    /// <summary>
	    /// Compare List String
	    /// </summary>
	    /// <param name="propertyName">name of property e.g. Readonly folders</param>
	    /// <param name="sourceIndexItem">source object</param>
	    /// <param name="oldListStringValue">oldListStringValue to compare with newListStringValue</param>
	    /// <param name="newListStringValue">newListStringValue to compare with oldListStringValue</param>
	    /// <param name="differenceList">list of different values</param>
	    private static void CompareListString(string propertyName, AppSettings sourceIndexItem, 
		    List<string> oldListStringValue, List<string> newListStringValue, List<string> differenceList)
	    {
		    if ( oldListStringValue == null || newListStringValue.Count == 0 ) return;
		    if ( oldListStringValue.Equals(newListStringValue) ) return;
		    
		    sourceIndexItem.GetType().GetProperty(propertyName)?.SetValue(sourceIndexItem, newListStringValue, null);
		    differenceList.Add(propertyName.ToLowerInvariant());
	    }
	    
	    /// <summary>
	    /// Compare List AppSettingsPublishProfiles
	    /// </summary>
	    /// <param name="propertyName">name of property e.g. PublishProfiles</param>
	    /// <param name="sourceIndexItem">source object</param>
	    /// <param name="oldListPublishValue">oldListPublishValue to compare with newListPublishValue</param>
	    /// <param name="newListPublishValue">newListPublishValue to compare with oldListPublishValue</param>
	    /// <param name="differenceList">list of different values</param>
	    private static void CompareListPublishProfiles(string propertyName, AppSettings sourceIndexItem, 
		    List<AppSettingsPublishProfiles> oldListPublishValue, List<AppSettingsPublishProfiles> newListPublishValue, List<string> differenceList)
	    {
		    if ( oldListPublishValue == null || newListPublishValue.Count == 0 ) return;
		    if ( oldListPublishValue.Equals(newListPublishValue) ) return;
		    
		    sourceIndexItem.GetType().GetProperty(propertyName)?.SetValue(sourceIndexItem, newListPublishValue, null);
		    differenceList.Add(propertyName.ToLowerInvariant());
	    }
	    
	    /// <summary>
	    /// Compare bool type 
	    /// </summary>
	    /// <param name="propertyName">name of property e.g. AddSwagger</param>
	    /// <param name="sourceIndexItem">source object</param>
	    /// <param name="oldBoolValue">oldBoolValue to compare with newBoolValue</param>
	    /// <param name="newBoolValue">oldBoolValue to compare with newBoolValue</param>
	    /// <param name="differenceList">list of different values</param>
        private static void CompareBool(string propertyName, AppSettings sourceIndexItem, bool oldBoolValue, 
		    bool newBoolValue, List<string> differenceList)
        {
            if (oldBoolValue == newBoolValue) return;
            sourceIndexItem.GetType().GetProperty(propertyName)?.SetValue(sourceIndexItem, newBoolValue, null);
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
        private static void CompareString(string propertyName, AppSettings sourceIndexItem, 
		    string oldStringValue, string newStringValue, 
		    List<string> differenceList)
        {
	        var newAppSettings = new AppSettings();

	        var defaultValue = GetPropertyValue(newAppSettings, propertyName) as string;
	        if ( newStringValue == defaultValue ) return;
	        
            if (oldStringValue == newStringValue ||
                (string.IsNullOrEmpty(newStringValue) )) return;
         
            var propertyObject = sourceIndexItem.GetType().GetProperty(propertyName);
            
	        propertyObject.SetValue(sourceIndexItem, newStringValue, null);
            
            differenceList.Add(propertyName.ToLowerInvariant());
        }
	    
	    /// <summary>
	    /// @see: https://stackoverflow.com/a/5508068
	    /// </summary>
	    /// <param name="car"></param>
	    /// <param name="propertyName"></param>
	    /// <returns></returns>
	    private static object GetPropertyValue(object car, string propertyName)
	    {
		    return car.GetType().GetProperties()
			    .Single(pi => pi.Name == propertyName)
			    .GetValue(car, null);
	    }

    }
}
