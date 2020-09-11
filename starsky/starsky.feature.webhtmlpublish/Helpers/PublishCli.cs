using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

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
		private readonly IStorage _subPathStorage;

		public PublishCli(ISelectorStorage storageSelector, IPublishPreflight publishPreflight,
			IWebHtmlPublishService publishService, AppSettings appSettings, IConsole console)
		{
			_publishPreflight = publishPreflight;
			_publishService = publishService;
			_appSettings = appSettings;
			_console = console;
			_argsHelper = new ArgsHelper(appSettings, console);
			_hostFileSystemStorage = storageSelector.Get(SelectorStorage.StorageServices.HostFilesystem);
			_subPathStorage = storageSelector.Get(SelectorStorage.StorageServices.SubPath);
		}
		
		/// <summary>
		/// Command Line Helper to server WebHtml Content
		/// </summary>
		/// <param name="args">arguments to adjust settings, see starskywebhtml/readme.md for more details</param>
		/// <returns>void</returns>
		public async Task Publisher(string[] args)
		{
			_appSettings.Verbose = _argsHelper.NeedVerbose(args);
			
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
				_console.WriteLine("Please add a valid folder: " + inputFullPath + ". " +
				                   "This folder is not found");
				return;
			}
			
			var settingsFullFilePath = Path.Combine(inputFullPath, "_settings.json");
			if ( _hostFileSystemStorage.ExistFile(settingsFullFilePath) )
			{
				_console.WriteLine($"You have already run this program for this folder remove the " +
				                   $"_settings.json first and try it again"  );
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
			      
			var fileIndexList = new ReadMeta(_subPathStorage)
				.ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);
			         
			// todo introduce selector
			var profileName = new PublishPreflight(_appSettings,_console)
				.GetPublishProfileNameByIndex(0);
			
			await _publishService.RenderCopy(fileIndexList,  profileName, 
				itemName, inputFullPath, true);
			
			_console.WriteLine("publish done");
		}
	}
}
