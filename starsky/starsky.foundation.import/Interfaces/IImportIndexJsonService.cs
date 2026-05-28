using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.import.Interfaces;

public interface IImportIndexJsonService
{
	Task<string> ExportAsync(string outputJsonPath, 
		SelectorStorage.StorageServices type =  SelectorStorage.StorageServices.HostFilesystem);
	Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath, 
		SelectorStorage.StorageServices type =  SelectorStorage.StorageServices.HostFilesystem);
}
