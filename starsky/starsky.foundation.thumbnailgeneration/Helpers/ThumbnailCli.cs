using System;
using System.Threading.Tasks;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Helpers;

[Obsolete("refactor to new Cli")]
public sealed class ThumbnailCli
{
	private readonly AppSettings _appSettings;
	private readonly IConsole _console;
	private readonly ISelectorStorage _selectorStorage;
	private readonly IThumbnailCleaner _thumbnailCleaner;
	private readonly IThumbnailService _thumbnailService;

	public ThumbnailCli(AppSettings appSettings,
		IConsole console, IThumbnailService thumbnailService,
		IThumbnailCleaner thumbnailCleaner,
		ISelectorStorage selectorStorage)
	{
		_appSettings = appSettings;
		_thumbnailService = thumbnailService;
		_console = console;
		_thumbnailCleaner = thumbnailCleaner;
		_selectorStorage = selectorStorage;
	}

	public async Task Thumbnail(string[] args)
	{
		_appSettings.Verbose = ArgsHelper.NeedVerbose(args);
		_appSettings.ApplicationType = AppSettings.StarskyAppType.Thumbnail;

		if ( ArgsHelper.NeedHelp(args) )
		{
			new ArgsHelper(_appSettings, _console).NeedHelpShowDialog();
			return;
		}

		new ArgsHelper().SetEnvironmentByArgs(args);

		var subPath = new ArgsHelper(_appSettings).SubPathOrPathValue(args);
		var getSubPathRelative = new ArgsHelper(_appSettings).GetRelativeValue(args);
		if ( getSubPathRelative != null )
		{
			subPath = new StructureService(_selectorStorage.Get(
					SelectorStorage.StorageServices.SubPath), _appSettings.Structure)
				.ParseSubfolders(getSubPathRelative)!;
		}

		if ( ArgsHelper.GetThumbnail(args) )
		{
			if ( _appSettings.IsVerbose() )
			{
				_console.WriteLine($">> GetThumbnail True ({DateTime.UtcNow:HH:mm:ss})");
			}

			var storage = _selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

			var isFolderOrFile = storage.IsFolderOrFile(subPath);

			if ( _appSettings.IsVerbose() )
			{
				_console.WriteLine(isFolderOrFile.ToString());
			}

			await _thumbnailService.GenerateThumbnail(subPath);

			_console.WriteLine($"Thumbnail Done! ({DateTime.UtcNow:HH:mm:ss})");
		}

		if ( ArgsHelper.NeedCleanup(args) )
		{
			_console.WriteLine(
				$"Next: Start Thumbnail Cache cleanup (-x true) ({DateTime.UtcNow:HH:mm:ss})");
			await _thumbnailCleaner.CleanAllUnusedFilesAsync();
			_console.WriteLine($"Cleanup Done! ({DateTime.UtcNow:HH:mm:ss})");
		}
	}
}
