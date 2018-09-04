using System.Collections.Generic;
using System.Reflection;
using starsky.Models;

namespace starsky.Helpers
{
    public static class FileIndexCompareHelper
    {
        public static List<string> Compare(FileIndexItem sourceIndexItem, FileIndexItem updateObject, bool append = false)
        {
            PropertyInfo[] propertiesA = sourceIndexItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] propertiesB = updateObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);


            int count = propertiesA.Length;
            var differenceList = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if ((propertiesA [i].CanRead) && (propertiesB [i].CanRead))
                {
                    if (propertiesA [i].PropertyType == typeof(string))
                    {
                        var oldStringValue = (string)propertiesA[i].GetValue(sourceIndexItem, null);
                        var newStringValue = (string)propertiesB[i].GetValue(updateObject, null);
                        if(oldStringValue != newStringValue && (!string.IsNullOrEmpty(newStringValue) || newStringValue == "/") )
                        {
                            if (propertiesB[i].Name != "FileName" )
                            {
                                var propertyObject = sourceIndexItem.GetType().GetProperty(propertiesB[i].Name);
                                
                                if (!append)
                                {
                                    propertyObject.SetValue(sourceIndexItem, newStringValue, null);
                                }
                                else if (propertiesB[i].Name == "Tags")
                                {
                                    propertyObject.SetValue(sourceIndexItem, oldStringValue + ", " + newStringValue,null);
                                }
                                else
                                {
                                    propertyObject.SetValue(sourceIndexItem,oldStringValue + newStringValue,null);
                                }
                            }
                            differenceList.Add(propertiesB[i].Name);
                        }
                    }

                    if (propertiesA [i].PropertyType == typeof(bool))
                    {
                        var oldBoolValue = (bool)propertiesA [i].GetValue(sourceIndexItem, null);
                        var newBoolValue = (bool)propertiesB [i].GetValue(updateObject, null);
                        if(oldBoolValue != newBoolValue)
                        {
                            sourceIndexItem.GetType().GetProperty(propertiesB[i].Name).SetValue(sourceIndexItem, newBoolValue, null);
                            differenceList.Add(propertiesB[i].Name);
                        }
                    }
                    
                    if (propertiesA [i].PropertyType == typeof(FileIndexItem.Color))
                    {
                        var oldColorValue = (FileIndexItem.Color)propertiesA [i].GetValue(sourceIndexItem, null);
                        var newColorValue = (FileIndexItem.Color)propertiesB [i].GetValue(updateObject, null);
                        if(oldColorValue != newColorValue)
                        {
                            sourceIndexItem.GetType().GetProperty(propertiesB[i].Name).SetValue(sourceIndexItem, newColorValue, null);
                            differenceList.Add(propertiesB[i].Name);
                        }
                    }
                }
            }
            return differenceList;
        }
    }
}