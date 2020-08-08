using System.Linq;
using System.Threading.Tasks;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskycore.Models;

namespace starsky.feature.import.Services
{
	public class ImportCli
	{
		public async Task Importer(string[] args, IImport importService, AppSettings appSettings, IConsole console)
		{
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
			
			if (new ArgsHelper().NeedHelp(args) || new ArgsHelper(appSettings).GetPathFormArgs(args,false).Length <= 1)
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.Importer;
				new ArgsHelper(appSettings, console).NeedHelpShowDialog();
				return;
			}
            
			var inputPathListFormArgs = new ArgsHelper(appSettings).GetPathListFormArgs(args);
			
			if ( appSettings.Verbose ) foreach ( var inputPath in inputPathListFormArgs )
			{
				console.WriteLine($">> import: {inputPath}");
			}
			
			var importSettings = new ImportSettingsModel {
					DeleteAfter = new ArgsHelper(appSettings).GetMove(args),
					RecursiveDirectory = new ArgsHelper().NeedRecursive(args),
					IndexMode = new ArgsHelper().GetIndexMode(args),
					ColorClass = new ArgsHelper().GetColorClass(args),
				};

			if ( appSettings.Verbose ) 
			{
				console.WriteLine($"Options: DeleteAfter: {importSettings.DeleteAfter}, " +
				                  $"RecursiveDirectory {importSettings.RecursiveDirectory}, " +
				                  $"ColorClass (overwrite) {importSettings.ColorClass}, " +
				                  $"Structure {appSettings.Structure}, " +
				                  $"IndexMode {importSettings.IndexMode}");
			}

			var result = await importService.Importer(inputPathListFormArgs, importSettings);
			
			console.WriteLine($"\nDone Importing {result.Count(p => p.Status == ImportStatus.Ok)}");
			console.WriteLine($"Failed: {result.Count(p => p.Status != ImportStatus.Ok)}");
		}
	}
}
