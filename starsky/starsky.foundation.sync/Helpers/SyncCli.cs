using System;
using System.Threading.Tasks;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.Helpers
{
	public class SyncCli
	{
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly ISynchronize _synchronize;
		private readonly ISelectorStorage _selectorStorage;

		public SyncCli(ISynchronize synchronize, AppSettings appSettings, IConsole console, ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_console = console;
			_synchronize = synchronize;
			_selectorStorage = selectorStorage;
		}
		
		public async Task Sync(string[] args)
		{
			_appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			if (new ArgsHelper().NeedHelp(args))
			{
				_appSettings.ApplicationType = AppSettings.StarskyAppType.Sync;
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

			if (new ArgsHelper().GetIndexMode(args))
			{
				_console.WriteLine($"Start indexing {subPath}");
				await _synchronize.Sync(subPath);
				_console.WriteLine("Done SyncFiles!");
			}

			_console.WriteLine("Done!");
		}
	}
}
