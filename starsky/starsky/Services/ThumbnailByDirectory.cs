﻿using System;
using starsky.Models;

namespace starsky.Services
{
    // This feature is used to crawl over directories and add this to the thumbnail-folder
    public static class ThumbnailByDirectory
    {
        public static void CreateThumb(string subpath = "/")
        {
            // Thumbnail check service
            var subFoldersFullPath =  Files.GetAllFilesDirectory(subpath);

            foreach (var singleFolderFullPath in subFoldersFullPath)
            {
                string[] filesInDirectoryFullPath = Files.GetFilesInDirectory(singleFolderFullPath,false);
                var localFileListFileHash = FileHash.GetHashCode(filesInDirectoryFullPath);

                for (int i = 0; i < filesInDirectoryFullPath.Length; i++)
                {
                    var value = new FileIndexItem()
                    {
                        FilePath = FileIndexItem.FullPathToDatabaseStyle(filesInDirectoryFullPath[i]),
                        FileHash = localFileListFileHash[i]
                    };
                    
                    if(AppSettingsProvider.Verbose) Console.WriteLine("localFileListFileHash[i] " + localFileListFileHash[i]); 
                    
                    Thumbnail.CreateThumb(value);
                }

                if (filesInDirectoryFullPath.Length >= 1)
                {
                    Console.WriteLine("~ " + filesInDirectoryFullPath.Length + " ~ "+  FileIndexItem.FullPathToDatabaseStyle(singleFolderFullPath));
                }

            }
        }
    }
}