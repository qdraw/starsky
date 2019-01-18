using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
    public partial class SyncService : ISync
    {
        private readonly ApplicationDbContext _context;
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;
        private readonly IReadMeta _readMeta;

		/// <summary>Do a sync of files uning a subpath</summary>
		/// <param name="context">Database Entity Framework context</param>
		/// <param name="query">Starsky IQuery interface to do calls on the database</param>
		/// <param name="appSettings">Settings of the application</param>
		/// <param name="readMeta">To read exif and xmp</param>
		public SyncService(ApplicationDbContext context, IQuery query, AppSettings appSettings, IReadMeta readMeta)
        {
            _context = context;
            _query = query;
            _appSettings = appSettings;
            _readMeta = readMeta;
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
        
//        [ExcludeFromCoverage]
        public IEnumerable<string> SyncFiles(string subPath, bool recursive = true)
        {
            // Prefix / for database
            subPath = ConfigRead.PrefixDbSlash(subPath);
            
            // Handle single files
            if (Deleted(subPath)) return null;
            
            // Return values when importing a single file
            var singleFile = SingleFile(subPath);
            switch (singleFile)
            {
                case SyncService.SingleFileSuccess.Fail:
                    return new List<string>{string.Empty};
                case SyncService.SingleFileSuccess.Success:
                    return new List<string>{subPath};
            }

            // Handle folder Get a list of all local folders and rename it to database style.
            // Db Style is a relative path
            var localSubFolderDbStyle = RenameListItemsToDbStyle(
                Files.GetAllFilesDirectory(_appSettings.DatabasePathToFilePath(subPath)).ToList()
            );

            // Query the database to get a list of the folder items
            var databaseSubFolderList = _query.GetAllFolders();
            
            // Check if the database folder list has no duplicates > delete them
            databaseSubFolderList = RemoveDuplicate(databaseSubFolderList);

            // Sync for folders
            // First remove old paths for the folders
            RemoveOldFilePathItemsFromDatabase(localSubFolderDbStyle, databaseSubFolderList, subPath);
            // Add new paths to database
            AddFoldersToDatabase(localSubFolderDbStyle, databaseSubFolderList);

            Console.WriteLine(".");
	        
			// dont crawl the content of the subfolders
			if (!recursive ) localSubFolderDbStyle = new List<string> ();
	        
            // Allow sync for the path the direct subPath for example '/2018', 
            localSubFolderDbStyle.Add(subPath);
				        
	        // Loop though the folders
			foreach (var singleFolder in localSubFolderDbStyle)
			{
				Console.Write(singleFolder + "  ");
				
				var databaseFileList = _query.GetAllFiles(singleFolder);
				var singleFolderFullPath = _appSettings.DatabasePathToFilePath(singleFolder);
				
				var localFarrayFilesFullFilePathStyle = Files.GetFilesInDirectory(singleFolderFullPath).ToList();
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
