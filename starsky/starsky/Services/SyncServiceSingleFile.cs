using System.Collections.Generic;
using starsky.Helpers;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // When input a direct file
        //        => if this file exist on the file system 
        //              => check if the hash in the db is up to date
        // Does not include parent folders

        // True is stop after
        // False is continue
        
        // Has support for subPaths in the index
        
        public bool SingleFile(string subPath = "")
        {
            if (Files.IsFolderOrFile(subPath) == FolderOrFileModel.FolderOrFileTypeList.File) // false == file
            {
                // File check if jpg #not corrupt
                var imageFormat = Files.GetImageFormat(FileIndexItem.DatabasePathToFilePath(subPath));
                if(imageFormat != Files.ImageFormat.jpg) return true;
                // todo: make fix for non-jpeg files
                // if(imageFormat != Files.ImageFormat.jpg) throw new BadImageFormatException("img != jpeg");
                
                // single file -- update or adding
                var dbListWithOneFile = new List<FileIndexItem>();
                var dbItem = _query.GetObjectByFilePath(subPath);
                if (dbItem != null)
                {
                    // If file already exist in database
                    dbListWithOneFile.Add(dbItem);
                }

                var localListWithOneFileDbStyle = new List<string>();
                localListWithOneFileDbStyle.Add(subPath);

                CheckMd5Hash(localListWithOneFileDbStyle, dbListWithOneFile);
                AddPhotoToDatabase(localListWithOneFileDbStyle, dbListWithOneFile);

                // add subpath
                AddSubPathFolder(subPath);
                
                return true;
            }
            return false;
        }
    }
}
