using System.Linq;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class PublishCli
	{
		private readonly IPublishPreflight _publishPreflight;
		private readonly IWebHtmlPublishService _publishService;
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly ArgsHelper _argsHelper;
		private readonly IStorage _hostFileSystemStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IStorage _subPathStorage;

		public PublishCli(ISelectorStorage storageSelector, IPublishPreflight publishPreflight,
			IWebHtmlPublishService publishService, AppSettings appSettings, IConsole console)
		{
			_publishPreflight = publishPreflight;
			_publishService = publishService;
			_appSettings = appSettings;
			_console = console;
			_argsHelper = new ArgsHelper(appSettings);
			_hostFileSystemStorage = storageSelector.Get(SelectorStorage.StorageServices.HostFilesystem);
			_thumbnailStorage = storageSelector.Get(SelectorStorage.StorageServices.Thumbnail);
			_subPathStorage = storageSelector.Get(SelectorStorage.StorageServices.SubPath);
		}
		public async Task Publisher(string[] args)
		{
			// Verbose is already defined
			
			if (_argsHelper.NeedHelp(args))
			{
				_appSettings.ApplicationType = AppSettings.StarskyAppType.WebHtml;
				_argsHelper.NeedHelpShowDialog();
				return;
			}
			
			var inputFullPath = _argsHelper.GetPathFormArgs(args,false);
			if (string.IsNullOrWhiteSpace(inputFullPath))
			{
				_console.WriteLine("Please use the -p to add a path first");
				return;
			}
			
			if ( _hostFileSystemStorage.IsFolderOrFile(inputFullPath) !=
			     FolderOrFileModel.FolderOrFileTypeList.Folder )
			{
				_console.WriteLine("Please add a valid folder: " + inputFullPath);
				return;
			}

			var itemName = _publishPreflight.GetNameConsole(inputFullPath,args);

			if(_appSettings.Verbose) _console.WriteLine("Name: " + itemName);
			if(_appSettings.Verbose) _console.WriteLine("inputPath " + inputFullPath);

			// used in this session to find the photos back
			// outside the webRoot of iStorage
			_appSettings.StorageFolder = inputFullPath;
			
			// use relative to StorageFolder
			var listOfFiles = _subPathStorage.GetAllFilesInDirectory("/")
				.Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();
			      
			var fileIndexList = new ReadMeta(_subPathStorage).ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
			         
			// Create thumbnails from the source images 
			new Thumbnail(_subPathStorage, _thumbnailStorage).CreateThumb("/"); // <= subPath style
			
			// Get base64 uri lists 
			var base64DataUri = new ToBase64DataUriList(_subPathStorage, _thumbnailStorage).Create(fileIndexList);
			
			// todo introduce selector
			var profileName = new PublishPreflight(_appSettings,_console).GetPublishProfileNameByIndex(0);
			
			await _publishService.RenderCopy(fileIndexList, base64DataUri, profileName, 
				itemName, inputFullPath, true);
		}
	}
}
