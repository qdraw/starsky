using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // For folders! only
        // If folder is not in database add folder element
        // loop though local file system

        public void AddFoldersToDatabase(List<string> localSubFolderDbStyle, List<FileIndexItem> databaseSubFolderList)
        {
            foreach (var singleFolderDbStyle in localSubFolderDbStyle)
            {

                // Check if Directory is in database
                var dbFolderMatchFirst = _context.FileIndex.FirstOrDefault(p =>
                    p.IsDirectory &&
                    p.FilePath == singleFolderDbStyle
                );

                // Folders!!!!
                if (dbFolderMatchFirst == null)
                {
                    var folderItem = new FileIndexItem();
                    folderItem.FilePath = singleFolderDbStyle;
                    folderItem.IsDirectory = true;
                    folderItem.AddToDatabase = DateTime.UtcNow;
                    folderItem.FileName = singleFolderDbStyle.Split("/").LastOrDefault();
                    folderItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(singleFolderDbStyle).LastOrDefault();
                    _query.AddItem(folderItem);
                    // We dont need this localy
                }

                // end folder
            }
        }

    }
}
