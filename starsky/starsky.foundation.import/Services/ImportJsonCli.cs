using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.import.Services;

[Service(typeof(IImportJsonCli), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImportJsonCli(
	IImportIndexJsonService importIndexJsonService,
	IWebLogger logger) : IImportJsonCli
{
	public async Task<bool> ImportExportByArgs(string[] args)
	{
		var importIndexExportJsonPath = ArgsHelper.GetImportIndexExportJsonPath(args);
		if ( !string.IsNullOrWhiteSpace(importIndexExportJsonPath) )
		{
			var exportLocation =
				await importIndexJsonService.ExportAsync(importIndexExportJsonPath);
			logger.LogInformation($"Exported ImportIndex to {exportLocation}");
			return true;
		}

		var importIndexImportJsonPath = ArgsHelper.GetImportIndexImportJsonPath(args);
		if ( string.IsNullOrWhiteSpace(importIndexImportJsonPath) )
		{
			return false;
		}

		var resultFromJson =
			await importIndexJsonService.ImportAsync(importIndexImportJsonPath);
		return resultFromJson.TrueForAll(p =>
			p.Status is ImportStatus.Ok or ImportStatus.IgnoredAlreadyImported);
	}
}
