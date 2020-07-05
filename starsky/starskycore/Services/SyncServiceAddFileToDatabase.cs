using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Services;

namespace starskycore.Services
{
    public partial class SyncService
    {
        // Add new file to database 
        //  (if file does not exist)

        public void AddFileToDatabase(
            List<string> localSubFolderDbStyle,
            List<FileIndexItem> databaseFileList
        )
        {
            foreach (var singleFolderDbStyle in localSubFolderDbStyle)
            {
                // Check if file is in database
                var dbFolderMatchFirst = databaseFileList.FirstOrDefault(
                    p =>
                        p.IsDirectory == false &&
                        p.FilePath == singleFolderDbStyle
                );

                if (dbFolderMatchFirst == null)
                {
                    // photo
                    Console.Write("+");
                    if(_appSettings.Verbose) Console.WriteLine("\nAddFileToDatabase: " + singleFolderDbStyle);


                    // Check the headers of a file to match a type
                    var imageFormat = ExtensionRolesHelper.GetImageFormat(_subPathStorage.ReadStream(singleFolderDbStyle,50));
                    
                    // Read data from file
	                var databaseItem = _readMeta.ReadExifAndXmpFromFile(singleFolderDbStyle);
	                databaseItem.ImageFormat = imageFormat;
	                databaseItem.SetAddToDatabase();
	                databaseItem.SetLastEdited();
                    databaseItem.FileHash = new FileHash(_subPathStorage).GetHashCode(singleFolderDbStyle).Key;
                    databaseItem.FileName = PathHelper.GetFileName(singleFolderDbStyle);
                    databaseItem.IsDirectory = false;
                    databaseItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(singleFolderDbStyle).LastOrDefault();
                        
                    _query.AddItem(databaseItem);
                    databaseFileList.Add(databaseItem);
                }

                // end new file
            }
        }
        
    }
}
