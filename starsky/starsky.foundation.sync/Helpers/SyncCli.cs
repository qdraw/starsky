using System;
using System.Diagnostics;
using System.Text;
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
	public sealed class SyncCli
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
			_appSettings.Verbose = ArgsHelper.NeedVerbose(args);
			_appSettings.ApplicationType = AppSettings.StarskyAppType.Sync;

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
				var parseSubPath = new StructureService(
						_selectorStorage.Get(SelectorStorage.StorageServices
							.SubPath), _appSettings.Structure)
					.ParseSubfolders(getSubPathRelative);
				if ( !string.IsNullOrEmpty(parseSubPath) )
				{
					subPath = parseSubPath;
				}
			}

			if ( ArgsHelper.GetIndexMode(args) )
			{
				var stopWatch = Stopwatch.StartNew();
				_console.WriteLine($"Start indexing {subPath}");
				var result = await _synchronize.Sync(subPath);
				if ( result.TrueForAll(p => p.FilePath != subPath) )
				{
					_console.WriteLine($"Not Found: {subPath}");
				}

				stopWatch.Stop();
				_console.WriteLine($"\nDone SyncFiles! {GetStopWatchText(stopWatch)}");
			}
			_console.WriteLine("Done!");
		}

		internal static string GetStopWatchText(Stopwatch stopWatch, int minMinutes = 3)
		{
			var timeText = new StringBuilder(
				$"(in sec: {Math.Round(stopWatch.Elapsed.TotalSeconds, 1)}");
			if ( stopWatch.Elapsed.TotalMinutes >= minMinutes )
			{
				timeText.Append($" or {Math.Round(stopWatch.Elapsed.TotalMinutes, 1)} min");
			}
			timeText.Append(") ");
			return timeText.ToString();
		}
	}
}
