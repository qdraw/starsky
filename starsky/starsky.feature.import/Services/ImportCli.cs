using System.Linq;
using System.Threading.Tasks;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Helpers;
using starskycore.Models;

namespace starsky.feature.import.Services
{
	public class ImportCli
	{
		private readonly IImport _importService;
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly IHttpClientHelper _httpClientHelper;

		public ImportCli(IImport importService, AppSettings appSettings, IConsole console, IHttpClientHelper httpClientHelper)
		{
			_importService = importService;
			_appSettings = appSettings;
			_console = console;
			_httpClientHelper = httpClientHelper;
		}
		
		/// <summary>
		/// Command line importer to Database and update disk
		/// </summary>
		/// <param name="args">arguments provided by command line app</param>
		/// <returns>Void Task</returns>
		public async Task Importer(string[] args)
		{
			_appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			await new ExifToolDownload(_httpClientHelper, _appSettings)
				.DownloadExifTool(_appSettings.IsWindows);
			
			if (new ArgsHelper().NeedHelp(args) || new ArgsHelper(_appSettings)
				.GetPathFormArgs(args,false).Length <= 1)
			{
				_appSettings.ApplicationType = AppSettings.StarskyAppType.Importer;
				new ArgsHelper(_appSettings, _console).NeedHelpShowDialog();
				return;
			}
            
			var inputPathListFormArgs = new ArgsHelper(_appSettings).GetPathListFormArgs(args);
			
			if ( _appSettings.Verbose ) foreach ( var inputPath in inputPathListFormArgs )
			{
				_console.WriteLine($">> import: {inputPath}");
			}
			
			var importSettings = new ImportSettingsModel {
					DeleteAfter = new ArgsHelper(_appSettings).GetMove(args),
					RecursiveDirectory = new ArgsHelper().NeedRecursive(args),
					IndexMode = new ArgsHelper().GetIndexMode(args),
					ColorClass = new ArgsHelper().GetColorClass(args),
				};

			if ( _appSettings.Verbose ) 
			{
				_console.WriteLine($"Options: DeleteAfter: {importSettings.DeleteAfter}, " +
				                  $"RecursiveDirectory {importSettings.RecursiveDirectory}, " +
				                  $"ColorClass (overwrite) {importSettings.ColorClass}, " +
				                  $"Structure {_appSettings.Structure}, " +
				                  $"IndexMode {importSettings.IndexMode}");
			}

			var result = await _importService.Importer(inputPathListFormArgs, importSettings);
			
			_console.WriteLine($"\nDone Importing {result.Count(p => p.Status == ImportStatus.Ok)}");
			_console.WriteLine($"Failed: {result.Count(p => p.Status != ImportStatus.Ok)}");
		}
	}
}
