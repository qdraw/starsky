using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.import.Interfaces;

public interface IImportIndexJsonService
{
	Task<string> ExportAsync(string outputJsonPath);
	Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath);
}