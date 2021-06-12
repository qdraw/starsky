using System;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starskycore.Interfaces;

namespace starskycore.Services
{
	public class SyncServiceCli
	{
		public void Sync(string[] args, ISync syncService, AppSettings appSettings, 
			IConsole console, IThumbnailCleaner thumbnailCleaner, ISelectorStorage selectorStorage)
		{
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			if (new ArgsHelper().NeedHelp(args))
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.Sync;
				new ArgsHelper(appSettings, console).NeedHelpShowDialog();
				return;
			}
			
			new ArgsHelper().SetEnvironmentByArgs(args);

			// This one is deprecated
			var subPath = new ArgsHelper(appSettings).SubPathOrPathValue(args);
			var getSubPathRelative = new ArgsHelper(appSettings).GetRelativeValue(args);
			if (getSubPathRelative != null)
			{
				subPath = new StructureService(selectorStorage.Get(SelectorStorage.StorageServices.SubPath), appSettings.Structure)
					.ParseSubfolders(getSubPathRelative);
			}

			if (new ArgsHelper().GetIndexMode(args))
			{
				console.WriteLine($"Start indexing {subPath}");
				syncService.SyncFiles(subPath);
				console.WriteLine("Done SyncFiles!");
			}

			if (new ArgsHelper(appSettings).GetThumbnail(args))
			{
				var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
				var thumbnailStorage =  selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

				var isFolderOrFile = storage.IsFolderOrFile(subPath);

				if (appSettings.Verbose) console.WriteLine(isFolderOrFile.ToString());
                
				if (isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File)
				{
					// If single file => create thumbnail
					var fileHash = new FileHash(storage).GetHashCode(subPath).Key;
					new Thumbnail(storage,thumbnailStorage, new WebLogger()).CreateThumb(subPath,fileHash); // <= this uses subPath
				}
				else
				{
					new Thumbnail(storage, thumbnailStorage, new WebLogger()).CreateThumb(subPath);
				}
                
				console.WriteLine("Thumbnail Done!");
			}
            
			if (new ArgsHelper(appSettings).GetOrphanFolderCheck(args))
			{
				console.WriteLine(">>>>> Heavy CPU Feature => Use with care <<<<< ");
				syncService.OrphanFolder(subPath);
			}

			if ( new ArgsHelper(appSettings).NeedCleanup(args) )
			{
				console.WriteLine(">>>>> Heavy CPU Feature => NeedCacheCleanup <<<<< ");
				thumbnailCleaner.CleanAllUnusedFiles();
			}

			console.WriteLine("Done!");
		}
	}
}
