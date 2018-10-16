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
        public List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList)
        {
            
            // Get a list of duplicate items
            var stackItemsByFileCollectionName = databaseSubFolderList.ToList().GroupBy(item => item.FileCollectionName)
                .SelectMany(grp => grp.Skip(1).Take(1)).ToList();
			// databaseSubFolderList.ToList() > Collection was modified; enumeration operation may not execute.
            
            // duplicateItemsByFilePath > 
            // If you have 3 item with the same name it will include 1 name
            // So we do a linq query to search simalar items
            // We keep the first item
            // And Delete duplicate items
            
            var querySubFolderList = new List<FileIndexItem>();
            // Do not remove it from: databaseSubFolderList otherwise it will be deleted from cache

            foreach (var stackItemByName in stackItemsByFileCollectionName)
            {
                var duplicateItems = databaseSubFolderList.Where(p => 
                    p.FileCollectionName == stackItemByName.FileCollectionName).ToList();
                // The idea to pick thumbnail based images first, followed by non-thumb supported
                // when not pick alphabetaly > todo implement this

                for (int i = 0; i < duplicateItems.Count; i++)
                {
                    if(Files.IsExtensionThumbnailSupported(duplicateItems[i].FileName))
                    {
                        querySubFolderList.Add(duplicateItems[i]);
                    }
                }

                if (querySubFolderList.Any(p => p.FileCollectionName == stackItemByName.FileCollectionName))
                {
                    // need to add docs
                    Console.WriteLine(">> Error code 4567890-098765");
                }

            }

            // Then add the items that are non duplicate back to the list
            foreach (var dbItem in databaseSubFolderList.ToList())
            {
                /// check if any item is duplicate
                if (stackItemsByFileCollectionName.All(p => 
                    p.FileCollectionName != dbItem.FileCollectionName))
                {
                    querySubFolderList.Add(dbItem);
                }
            }
            
            return querySubFolderList.OrderBy(p => p.FileName).ToList();
        }
    }
}