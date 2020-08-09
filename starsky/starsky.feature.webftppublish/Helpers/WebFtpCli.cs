using System.IO;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;
using starsky.feature.webftppublish.Models;
using starsky.feature.webftppublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.webftppublish.Helpers
{
	public class WebFtpCli
	{
		private readonly ArgsHelper _argsHelper;
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly IStorage _hostStorageProvider;
		private readonly IFtpWebRequestFactory _webRequestFactory;

		public WebFtpCli(AppSettings appSettings, ISelectorStorage selectorStorage, IConsole console, IFtpWebRequestFactory webRequestFactory)
		{
			_appSettings = appSettings;
			_console = console;
			_argsHelper = new ArgsHelper(_appSettings,console);
			_hostStorageProvider = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_webRequestFactory = webRequestFactory;
		}

		public void Run(string[] args)
		{
			_appSettings.Verbose = _argsHelper.NeedVerbose(args);
			
			if (_argsHelper.NeedHelp(args))
			{
				_appSettings.ApplicationType = AppSettings.StarskyAppType.WebHtml;
				_argsHelper.NeedHelpShowDialog();
				return;
			}
			
			var inputFullFileDirectory = new ArgsHelper(_appSettings)
				.GetPathFormArgs(args,false);

			if (string.IsNullOrWhiteSpace(inputFullFileDirectory))
			{
				_console.WriteLine("Please use the -p to add a path first");
				return;
			}
            
			// used in this session to find the files back

			if ( _hostStorageProvider.IsFolderOrFile(inputFullFileDirectory) 
			     == FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				_console.WriteLine($"Folder location {inputFullFileDirectory} " +
				                   $"is not found \nPlease try the `-h` command to get help ");
				return;
			}
			
			// check if settings is valid
			if ( string.IsNullOrEmpty(_appSettings.WebFtp) )
			{
				_console.WriteLine($"Please update the WebFtp settings in appsettings.json"  );
				return;
			}

			var settingsFullFilePath = Path.Combine(inputFullFileDirectory, "_settings.json");
			if ( !_hostStorageProvider.ExistFile(settingsFullFilePath) )
			{
				_console.WriteLine($"Please run 'starskywebhtmlcli' " +
				                   $"first to generate a settings file"  );
				return;
			}

			var settings =
				new DeserializeJson(_hostStorageProvider).Read<FtpPublishManifestModel>(
					settingsFullFilePath);

			var ftpService = new FtpService(_appSettings,_hostStorageProvider, 
					_console, _webRequestFactory)
				.Run(inputFullFileDirectory, settings.Slug, settings.Copy);

			if ( !ftpService )
			{
				_console.WriteLine("Ftp copy failed");
				return;
			};
			
			_console.WriteLine($"Ftp copy successful done: {settings.Slug}");
		}
	}
}
