using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Helpers
{
	public class ThumbnailCli
	{
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly IThumbnailCleaner _thumbnailCleaner;
		private readonly ISelectorStorage _selectorStorage;
		private readonly IThumbnailService _thumbnailService;

		public ThumbnailCli(AppSettings appSettings, 
			IConsole console, IThumbnailService thumbnailService, IThumbnailCleaner thumbnailCleaner, 
			ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_thumbnailService = thumbnailService;
			_console = console;
			_thumbnailCleaner = thumbnailCleaner;
			_selectorStorage = selectorStorage;
		}
		
		public void Thumbnail(string[] args)
		{
			_appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			if (new ArgsHelper().NeedHelp(args))
			{
				_appSettings.ApplicationType = AppSettings.StarskyAppType.Thumbnail;
				new ArgsHelper(_appSettings, _console).NeedHelpShowDialog();
				return;
			}
			
			new ArgsHelper().SetEnvironmentByArgs(args);

			var subPath = new ArgsHelper(_appSettings).SubPathOrPathValue(args);
			var getSubPathRelative = new ArgsHelper(_appSettings).GetRelativeValue(args);
			if (getSubPathRelative != null)
			{
				subPath = new StructureService(_selectorStorage.Get(SelectorStorage.StorageServices.SubPath), _appSettings.Structure)
					.ParseSubfolders(getSubPathRelative);
			}

			if (new ArgsHelper(_appSettings).GetThumbnail(args))
			{
				var storage = _selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

				var isFolderOrFile = storage.IsFolderOrFile(subPath);

				if (_appSettings.Verbose) _console.WriteLine(isFolderOrFile.ToString());
                
				if (isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.File)
				{
					// If single file => create thumbnail
					var fileHash = new FileHash(storage).GetHashCode(subPath).Key;
					_thumbnailService.CreateThumb(subPath, fileHash); // <= this uses subPath
				}
				else
				{
					_thumbnailService.CreateThumb(subPath);
				}
				_console.WriteLine("Thumbnail Done!");
			}
            
			if ( new ArgsHelper(_appSettings).NeedCleanup(args) )
			{
				_console.WriteLine(">>>>> Heavy CPU Feature => NeedCacheCleanup <<<<< ");
				_thumbnailCleaner.CleanAllUnusedFiles();
			}

			_console.WriteLine("Done!");
		}
	}
}
