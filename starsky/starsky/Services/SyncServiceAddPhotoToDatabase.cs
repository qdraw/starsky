using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // Add new photo to database 
        //  (if photo does not exist)

        public void AddPhotoToDatabase(
            List<string> localSubFolderDbStyle,
            List<FileIndexItem> databaseFileList
        )
        {
            foreach (var singleFolderDbStyle in localSubFolderDbStyle)
            {
                // Check if Photo is in database
                var dbFolderMatchFirst = databaseFileList.FirstOrDefault(
                    p =>
                        !p.IsDirectory &&
                        p.FilePath == singleFolderDbStyle
                );

                if (dbFolderMatchFirst == null)
                {
                    // photo
                    Console.Write(".");
                    if(_appSettings.Verbose) Console.WriteLine("\nAddPhotoToDatabase: " + singleFolderDbStyle);

                    var singleFilePath = _appSettings.DatabasePathToFilePath(singleFolderDbStyle);
                    var databaseItem = ExifRead.ReadExifFromFile(singleFilePath);

                    databaseItem.AddToDatabase = DateTime.UtcNow;
                    databaseItem.FileHash = FileHash.GetHashCode(singleFilePath);
                    databaseItem.FileName = Path.GetFileName(singleFilePath);
                    databaseItem.IsDirectory = false;
                    databaseItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(singleFolderDbStyle).LastOrDefault();

                    _query.AddItem(databaseItem);
                    databaseFileList.Add(databaseItem);
                }

                // end photo
            }
        }
    }
}
