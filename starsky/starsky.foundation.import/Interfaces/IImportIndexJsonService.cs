using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.import.Interfaces;

public interface IImportIndexJsonService
{
	[ExcludeFromCodeCoverage]
	Task<string> ExportAsync(string outputJsonPath, 
		SelectorStorage.StorageServices type =  SelectorStorage.StorageServices.HostFilesystem);
	
	[ExcludeFromCodeCoverage]
	Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath, 
		SelectorStorage.StorageServices type =  SelectorStorage.StorageServices.HostFilesystem);
}
