using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // Add duplicate check
        public List<FileIndexItem> RemoveDuplicate(List<FileIndexItem> databaseSubFolderList)
        {

            var duplicateItems = databaseSubFolderList.Where(x => x.FilePath.Length > 1).Select(x => x.FilePath).ToList();
            
            // Delete removed items
            foreach (var item in duplicateItems)
            {
                var ditem = databaseSubFolderList.FirstOrDefault(p => p.FilePath == item);
                databaseSubFolderList.Remove(ditem);
                Console.WriteLine();
                _query.RemoveItem(ditem);
            }

            return databaseSubFolderList;
        }

    }
}