﻿using starsky.Helpers;
using starsky.Models;

namespace starsky.Services
{
    // Gives folder an thumbnail image (only if contains direct files)
    // SyncServiceFirstItemDirectory
    public partial class SyncService
    {
        // Subpath is the base folder, it scans subfolders
        public void FirstItemDirectory(string subpath = "/")
        {
            subpath = _query.SubPathSlashRemove(subpath);

            // Loop though all folders
            var subFoldersFullPath = Files.GetAllFilesDirectory(subpath);

            foreach (var singleFolderFullPath in subFoldersFullPath)
            {
                string[] filesInDirectoryFullPath = Files.GetFilesInDirectory(singleFolderFullPath, false);

                if (filesInDirectoryFullPath.Length >= 1)
                {
                    var subPathSingleItem = 
                        FileIndexItem.FullPathToDatabaseStyle(filesInDirectoryFullPath[0]);
                    var dbItem = _query.GetObjectByFilePath(subPathSingleItem);
                    // Check if photo item exist in database
                    if (dbItem == null) return;
                    
                    // Check if parent folder exist in database
                    var dbParentItem = _query.GetObjectByFilePath(dbItem.ParentDirectory);
                    if (dbParentItem == null) return;
                    // get hash from file
                    var singleFileHash = FileHash.GetHashCode(filesInDirectoryFullPath[0]);
                    // compare both
                    if (dbParentItem.FileHash != singleFileHash)
                    {
                        dbParentItem.FileHash = singleFileHash;
                        _query.UpdateItem(dbParentItem);
                    }
                    
                }
            }
        }
    }
}