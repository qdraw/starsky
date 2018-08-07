﻿using System.Collections.Generic;
using System.IO;
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
        
        public string SingleFile(string subPath = "")
        {
            var fullFilePath = _appSettings.DatabasePathToFilePath(subPath);
            
            if (Files.IsFolderOrFile(fullFilePath) == FolderOrFileModel.FolderOrFileTypeList.File) // false == file
            {
                // File check if jpg #not corrupt
                var imageFormat = Files.GetImageFormat(fullFilePath);
                if(imageFormat == Files.ImageFormat.unknown) return string.Empty;
                
                // The same check as in GetFilesInDirectory
                var extension = Path.GetExtension(fullFilePath).ToLower().Replace(".",string.Empty);
                if (!Files.ExtensionSyncSupportedList.Contains(extension)) return string.Empty;
                 
                // single file -- update or adding
                var dbListWithOneFile = new List<FileIndexItem>();
                var dbItem = _query.GetObjectByFilePath(subPath);
                if (dbItem != null)
                {
                    // If file already exist in database
                    dbListWithOneFile.Add(dbItem);
                }

                var localListWithOneFileDbStyle = new List<string> {subPath};

                CheckMd5Hash(localListWithOneFileDbStyle, dbListWithOneFile);
                AddPhotoToDatabase(localListWithOneFileDbStyle, dbListWithOneFile);

                // add subpath
                AddSubPathFolder(subPath);
                
                return subPath;
            }
            return string.Empty;
        }
    }
}
