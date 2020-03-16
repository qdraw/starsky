using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.query.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	[Service(typeof(ISync), InjectionLifetime = InjectionLifetime.Scoped)]
	public partial class SyncService : ISync
    {
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;
        private readonly IReadMeta _readMeta;
	    private readonly IStorage _subPathStorage;

	    /// <summary>Do a sync of files uning a subpath</summary>
	    /// <param name="query">Starsky IQuery interface to do calls on the database</param>
	    /// <param name="appSettings">Settings of the application</param>
	    /// <param name="readMeta">To read exif and xmp</param>
	    /// <param name="selectorStorage">Filesystem abstraction</param>
	    public SyncService(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
        {
            _query = query;
            _appSettings = appSettings;
            _subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
            _readMeta = new ReadMeta(_subPathStorage,_appSettings);
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
        
        public IEnumerable<string> SyncFiles(string subPath, bool recursive = true)
        {
            // Prefix / for database
            subPath = PathHelper.PrefixDbSlash(subPath);
            
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
	        var localSubFolderDbStyle = _subPathStorage.GetDirectoryRecursive(subPath).ToList();

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
				var localFarrayFilesDbStyle = _subPathStorage.GetAllFilesInDirectory(singleFolder)
					.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();
				
				databaseFileList = RemoveDuplicate(databaseFileList);
				databaseFileList = RemoveOldFilePathItemsFromDatabase(localFarrayFilesDbStyle, databaseFileList, subPath);
				CheckMd5Hash(localFarrayFilesDbStyle, databaseFileList);
				AddFileToDatabase(localFarrayFilesDbStyle, databaseFileList);
				Console.WriteLine("-");
			}
	        
	        // Add the subpaths recruisivly 
	        AddSubPathFolder(subPath);
			
	        // Gives folder an thumbnail image (only if contains direct files)
	        FirstItemDirectory(subPath);
	        
	        return null;
        }


    }


}
