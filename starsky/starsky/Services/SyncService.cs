using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Attributes;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService : ISync
    {
        private readonly ApplicationDbContext _context;
        private readonly IQuery _query;

        public SyncService(ApplicationDbContext context, IQuery query)
        {
            _context = context;
            _query = query;
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
        
        
        [ExcludeFromCoverage] // The reason is because this is an index
        public IEnumerable<string> SyncFiles(string subPath = "")
        {
            // Handle single files
            if (Deleted(subPath)) return null;
            if (SingleFile(subPath)) return null;

            // Handle folder Get a list of all local folders and rename it to database style.
            // Db Style is a relative path
            var localSubFolderDbStyle = RenameListItemsToDbStyle(
                Files.GetAllFilesDirectory(subPath).ToList()
            );

            // Query the database to get a list of the folder items
            var databaseSubFolderList = _context.FileIndex.Where(p => p.IsDirectory).ToList();

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
                var localFarrayFilesDbStyle = Files.GetFilesInDirectory(singleFolder).ToList();

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
                localSubFolderListDatabaseStyle.Add(FileIndexItem.FullPathToDatabaseStyle(item));
            }

            return localSubFolderListDatabaseStyle;
        }

    }
}
