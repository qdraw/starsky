using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starskycore.Helpers;
using starskycore.Models;

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
                        !p.IsDirectory &&
                        p.FilePath == singleFolderDbStyle
                );

                if (dbFolderMatchFirst == null)
                {
                    // photo
                    Console.Write(".");
                    if(_appSettings.Verbose) Console.WriteLine("\nAddFileToDatabase: " + singleFolderDbStyle);


                    // Check the headers of a file to match a type
                    var imageFormat = ExtensionRolesHelper.GetImageFormat(_iStorage.ReadStream(singleFolderDbStyle,160));
                    
                    // Read data from file
	                var databaseItem = _readMeta.ReadExifAndXmpFromFile(singleFolderDbStyle);
	                databaseItem.ImageFormat = imageFormat;
	                databaseItem.SetAddToDatabase();
	                databaseItem.SetLastEdited();
                    databaseItem.FileHash = new FileHash(_iStorage).GetHashCode(singleFolderDbStyle);
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
