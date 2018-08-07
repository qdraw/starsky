using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Attributes;
using starsky.Data;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService : ISync
    {
        private readonly ApplicationDbContext _context;
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;

        public SyncService(ApplicationDbContext context, IQuery query, AppSettings appSettings)
        {
            _context = context;
            _query = query;
            _appSettings = appSettings;
        }
        
        /* Base feature to sync files and folders
        The filter you can use is subPath.
        For example if your directory structure look like this:
        /home/pi/images/2018
            /home/pi/images/2018/01
                .. here some files
        
        The base path can be '/home/pi/images'
        And the subpath can ben 2018 to crawl only files inside this folder
        */
        
        [ExcludeFromCoverage]
        public IEnumerable<string> SyncFiles(string subPath)
        {
            // Handle single files
            if (Deleted(subPath)) return null;
            if (!string.IsNullOrEmpty(SingleFile(subPath))) return new List<string>{subPath};

            // Handle folder Get a list of all local folders and rename it to database style.
            // Db Style is a relative path
            var localSubFolderDbStyle = RenameListItemsToDbStyle(
                Files.GetAllFilesDirectory(_appSettings.DatabasePathToFilePath(subPath)).ToList()
            );

            // Query the database to get a list of the folder items
            var databaseSubFolderList = _context.FileIndex.Where(p => p.IsDirectory).ToList();
            
            // Check if the database folder list has no duplicates > delete them
            databaseSubFolderList = RemoveDuplicate(databaseSubFolderList);

            // Sync for folders
            // First remove old paths for the folders
            RemoveOldFilePathItemsFromDatabase(localSubFolderDbStyle, databaseSubFolderList, subPath);
            // Add new paths to database
            AddFoldersToDatabase(localSubFolderDbStyle, databaseSubFolderList);

            Console.WriteLine(".");

            // Allow sync for the path the direct subPath for example '/2018', 
            localSubFolderDbStyle.Add(subPath);

            // Loop though the folders
            foreach (var singleFolder in localSubFolderDbStyle)
            {
                Console.Write(singleFolder + "  ");

                var databaseFileList = _query.GetAllFiles(singleFolder);
                var singleFolderFullPath = _appSettings.DatabasePathToFilePath(singleFolder);
                var localFarrayFilesFullFilePathStyle = Files.GetFilesInDirectory(singleFolderFullPath,_appSettings).ToList();
                var localFarrayFilesDbStyle = RenameListItemsToDbStyle(localFarrayFilesFullFilePathStyle); 
                databaseFileList = RemoveDuplicate(databaseFileList);
                databaseFileList = RemoveOldFilePathItemsFromDatabase(localFarrayFilesDbStyle, databaseFileList, subPath);
                CheckMd5Hash(localFarrayFilesDbStyle, databaseFileList);
                AddPhotoToDatabase(localFarrayFilesDbStyle, databaseFileList);
                Console.WriteLine("-");
            }

            // Add the subpaths recruisivly 
            AddSubPathFolder(subPath);
            
            // Gives folder an thumbnail image (only if contains direct files)
            FirstItemDirectory(subPath);

            return null;
        }

        // Rename a list to database style (short style)
        public List<string> RenameListItemsToDbStyle(List<string> localSubFolderList)
        {
            var localSubFolderListDatabaseStyle = new List<string>();

            foreach (var item in localSubFolderList)
            {
                localSubFolderListDatabaseStyle.Add(_appSettings.FullPathToDatabaseStyle(item));
            }

            return localSubFolderListDatabaseStyle;
        }

    }
}
