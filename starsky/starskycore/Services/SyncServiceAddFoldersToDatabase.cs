using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Models;

namespace starskycore.Services
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
                if(_appSettings.Verbose) Console.WriteLine(singleFolderDbStyle);

                // Check if Directory is in database
                var dbFolderMatchFirst = databaseSubFolderList.FirstOrDefault(p =>
                    p.IsDirectory &&
                    p.FilePath == singleFolderDbStyle
                );

                // Folders!!!!
                if (dbFolderMatchFirst == null)
                {
                    var folderItem = new FileIndexItem
                    {
                        IsDirectory = true,
                        AddToDatabase = DateTime.UtcNow,
                        FileName = singleFolderDbStyle.Split("/".ToCharArray()).LastOrDefault(),
                        ParentDirectory = Breadcrumbs.BreadcrumbHelper(singleFolderDbStyle).LastOrDefault(),
                        ColorClass = FileIndexItem.Color.None
                    };
                    _query.AddItem(folderItem);
                    // We dont need this localy
                }

                // end folder
            }
        }

    }
}
