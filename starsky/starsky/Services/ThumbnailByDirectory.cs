using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Helpers;
using starsky.Models;

namespace starsky.Services
{
    // This feature is used to crawl over directories and add this to the thumbnail-folder
    public class ThumbnailByDirectory
    {
        private readonly AppSettings _appSettings;

        public ThumbnailByDirectory(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        
        public string[] ToBase64DataUriList(List<FileIndexItem> fileIndexList)
        {
            var base64ImageArray = new string[fileIndexList.Count];
            for (var i = 0; i<fileIndexList.Count; i++)
            {
                var item = fileIndexList[i];
                var fullFilePath = _appSettings.DatabasePathToFilePath(item.FilePath);
                base64ImageArray[i] = "data:image/png;base64," + Base64Helper
                                          .ToBase64(new Thumbnail(null).ResizeThumbnailToStream(fullFilePath, 4, 0, 0, true,
                                              Files.ImageFormat.png));
            }
            return base64ImageArray;
        }

        public void CreateThumb(string parentFolderFullPath = "/")
        {
            // Thumbnail check service
            
            var subFoldersFullPathList =  Files.GetAllFilesDirectory(parentFolderFullPath).ToList();
            
            // Add Subpath to scan the root folder for thumbs       
            subFoldersFullPathList.Add(parentFolderFullPath);
            
            foreach (var singleFolderFullPath in subFoldersFullPathList)
            {
                string[] filesInDirectoryFullPath = Files.GetFilesInDirectory(singleFolderFullPath);
                var localFileListFileHash = FileHash.GetHashCode(filesInDirectoryFullPath);

                for (int i = 0; i < filesInDirectoryFullPath.Length; i++)
                {
                    var value = new FileIndexItem()
                    {
                        ParentDirectory = Breadcrumbs.BreadcrumbHelper(_appSettings.FullPathToDatabaseStyle(filesInDirectoryFullPath[i])).LastOrDefault(),
                        FileName = Path.GetFileName(_appSettings.FullPathToDatabaseStyle(filesInDirectoryFullPath[i])),
                        FileHash = localFileListFileHash[i]
                    };

                    if (_appSettings.Verbose) Console.WriteLine("localFileListFileHash[i] " + localFileListFileHash[i]); 
                    
                    new Thumbnail(_appSettings,_exiftool).CreateThumb(value);
                }

                if (filesInDirectoryFullPath.Length >= 1)
                {
                    Console.WriteLine("~ " + filesInDirectoryFullPath.Length + " ~ "+  _appSettings.FullPathToDatabaseStyle(singleFolderFullPath));
                }

            }

        }
    }
}