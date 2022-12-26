using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.metathumbnail.Helpers
{
	public class MetaThumbnailCommandLineHelper
	{
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly IMetaExifThumbnailService _metaExifThumbnailService;
		private readonly ISelectorStorage _selectorStorage;
		private readonly IMetaUpdateStatusThumbnailService _statusThumbnailService;

		public MetaThumbnailCommandLineHelper(ISelectorStorage selectorStorage, AppSettings appSettings, 
			IConsole console, IMetaExifThumbnailService metaExifThumbnailService, IMetaUpdateStatusThumbnailService statusThumbnailService)
		{
			_selectorStorage = selectorStorage;
			_appSettings = appSettings;
			_metaExifThumbnailService = metaExifThumbnailService;
			_console = console;
			_statusThumbnailService = statusThumbnailService;
		}
		
		public async Task CommandLineAsync(string[] args)
		{
			_appSettings.Verbose = ArgsHelper.NeedVerbose(args);
			_appSettings.ApplicationType = AppSettings.StarskyAppType.MetaThumbnail;

			if (ArgsHelper.NeedHelp(args))
			{
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

			var statusResultsWithSubPaths = await _metaExifThumbnailService.AddMetaThumbnail(subPath);
			_console.WriteLine("next: run update status");
			await _statusThumbnailService.UpdateStatusThumbnail(statusResultsWithSubPaths);
			_console.WriteLine("Done!");
		}
	}
}

