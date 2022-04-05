using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Models;

namespace starsky.feature.import.Services
{
	public class ImportCli
	{
		private readonly IImport _importService;
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly IExifToolDownload _exifToolDownload;

		public ImportCli(IImport importService, AppSettings appSettings, IConsole console, IExifToolDownload exifToolDownload)
		{
			_importService = importService;
			_appSettings = appSettings;
			_console = console;
			_exifToolDownload = exifToolDownload;
		}
		
		/// <summary>
		/// Command line importer to Database and update disk
		/// </summary>
		/// <param name="args">arguments provided by command line app</param>
		/// <returns>Void Task</returns>
		public async Task Importer(string[] args)
		{
			Console.WriteLine("run importer");
			_appSettings.Verbose = ArgsHelper.NeedVerbose(args);

			await _exifToolDownload.DownloadExifTool(_appSettings.IsWindows);
			_appSettings.ApplicationType = AppSettings.StarskyAppType.Importer;

			if (new ArgsHelper().NeedHelp(args) || new ArgsHelper(_appSettings)
				.GetPathFormArgs(args,false).Length <= 1)
			{
				new ArgsHelper(_appSettings, _console).NeedHelpShowDialog();
				return;
			}
            
			var inputPathListFormArgs = new ArgsHelper(_appSettings).GetPathListFormArgs(args);
			
			if ( _appSettings.IsVerbose() ) foreach ( var inputPath in inputPathListFormArgs )
			{
				_console.WriteLine($">> import: {inputPath}");
			}
			
			var importSettings = new ImportSettingsModel {
				DeleteAfter = ArgsHelper.GetMove(args),
				RecursiveDirectory = ArgsHelper.NeedRecursive(args),
				IndexMode = new ArgsHelper().GetIndexMode(args),
				ColorClass = ArgsHelper.GetColorClass(args),
				ConsoleOutputMode = ArgsHelper.GetConsoleOutputMode(args)
			};

			if ( _appSettings.IsVerbose() ) 
			{
				_console.WriteLine($"Options: DeleteAfter: {importSettings.DeleteAfter}, " +
				                  $"RecursiveDirectory {importSettings.RecursiveDirectory}, " +
				                  $"ColorClass (overwrite) {importSettings.ColorClass}, " +
				                  $"Structure {_appSettings.Structure}, " +
				                  $"IndexMode {importSettings.IndexMode}");
			}
			
			var stopWatch = Stopwatch.StartNew();
			var result = await _importService.Importer(inputPathListFormArgs, importSettings);

			WriteOutputStatus(importSettings, result, stopWatch);
			Console.WriteLine("done import");

		}

		private void WriteOutputStatus(ImportSettingsModel importSettings, List<ImportIndexItem> result, Stopwatch stopWatch)
		{
			if ( importSettings.IsConsoleOutputModeDefault() )
			{
				var okCount = result.Count(p => p.Status == ImportStatus.Ok);
				_console.WriteLine($"\nDone Importing {okCount}");
				if ( okCount != 0 ) {
					_console.WriteLine($"Time: {Math.Round(stopWatch.Elapsed.TotalSeconds, 1)} " +
					                   $"sec. or {Math.Round(stopWatch.Elapsed.TotalMinutes, 1)} min.");
				}
				_console.WriteLine($"Failed: {result.Count(p => p.Status != ImportStatus.Ok)}");
			}

			if ( importSettings.ConsoleOutputMode != ConsoleOutputMode.Csv )
			{
				return;
			}
			
			_console.WriteLine($"Id;Status;SourceFullFilePath;SubPath;FileHash");
			foreach ( var item in result )
			{
				var filePath = item.Status == ImportStatus.Ok
					? item.FilePath : "";
				_console.WriteLine($"{item.Id};{item.Status};" +
				                   $"{item.SourceFullFilePath};" +
				                   $"{filePath};" +
				                   $"{item.GetFileHashWithUpdate()}");

			}
		}
	}
}
