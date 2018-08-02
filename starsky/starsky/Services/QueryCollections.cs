using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Helpers;
using starsky.Models;

namespace starsky.Services
{
    public partial class Query
    {
        public List<FileIndexItem> Collections(List<FileIndexItem> databaseSubFolderList)
        {
            
            // Get a list of duplicate items
            var duplicateItemsByFilePath = databaseSubFolderList.GroupBy(item => item.FileCollectionName)
                .SelectMany(grp => grp.Skip(1).Take(1)).ToList();
            
            // duplicateItemsByFilePath > 
            // If you have 3 item with the same name it will include 1 name;
            // So we do a linq query to search simalar items
            // We keep the first item
            // And Delete duplicate items
            
            foreach (var duplicateItemByName in duplicateItemsByFilePath)
            {
                var duplicateItems = databaseSubFolderList.Where(p => p.FileCollectionName == duplicateItemByName.FileCollectionName).ToList();
                // The idea to pick thumbnail based images first, followed by non-thumb supported
                // when not pick alphabetaly > todo implement this

                for (int i = 0; i < duplicateItems.Count(); i++)
                {
                    var fileExtension = Path.GetExtension(duplicateItems[i].FileName).Replace(".",string.Empty);

                    if (!Files.ExtensionThumbSupportedList.Contains(fileExtension))
                    {
                        databaseSubFolderList.Remove(duplicateItems[i]);
                    }
                }

            }
            return databaseSubFolderList;
        }
    }
}