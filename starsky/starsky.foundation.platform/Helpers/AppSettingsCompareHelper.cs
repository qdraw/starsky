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
        public static List<string> Compare(AppSettings sourceIndexItem, object updateObject = null)
        {
            if(updateObject == null) updateObject = new AppSettings();
            PropertyInfo[] propertiesA = sourceIndexItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] propertiesB = updateObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            var differenceList = new List<string>();
            foreach ( var propertyB in propertiesB )
            {
	            // only for when TransferObject is Nullable<bool> and AppSettings is bool
	            var propertyInfoFromA = propertiesA.FirstOrDefault(p => p.Name == propertyB.Name);
	            if (propertyInfoFromA != null && propertyInfoFromA.PropertyType == typeof(bool) && propertyB.PropertyType == typeof(bool?))
	            {
		            var oldBoolValue = (bool)propertyInfoFromA.GetValue(sourceIndexItem, null);
		            var newBoolValue = (bool?)propertyB.GetValue(updateObject, null);
		            CompareBool(propertyB.Name, sourceIndexItem, oldBoolValue, newBoolValue, differenceList);
	            }

	            if ( propertyInfoFromA != null && propertyB.PropertyType == typeof(string) )
	            {
		            var oldStringValue = (string)propertyInfoFromA.GetValue(sourceIndexItem, null);
		            var newStringValue = (string)propertyB.GetValue(updateObject, null);
		            CompareString(propertyB.Name, sourceIndexItem, oldStringValue, newStringValue, differenceList);
	            }
            }
            return differenceList;
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
		    bool? newBoolValue, List<string> differenceList)
        {
	        if ( newBoolValue == null ) return;
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
