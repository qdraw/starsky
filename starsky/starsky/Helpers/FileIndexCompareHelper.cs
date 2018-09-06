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
            }
            return differenceList;
        }

        private static void CompareColor(string propertyName, FileIndexItem sourceIndexItem, FileIndexItem.Color oldColorValue, FileIndexItem.Color newColorValue, List<string> differenceList)
        {
            if (oldColorValue == newColorValue) return;
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