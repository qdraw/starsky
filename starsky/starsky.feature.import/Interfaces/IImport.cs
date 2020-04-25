using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starskycore.Models;

namespace starsky.feature.import.Interfaces
{
    public interface IImport
    {
	    Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList,
		    ImportSettingsModel importSettings);
	    
        List<string> ImportTo(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings);
        
        List<string> ImportTo(string inputFullPathList, ImportSettingsModel importSettings);

	    // List<ImportIndexItem> Preflight(List<string> inputFileFullPaths, ImportSettingsModel importSettings);
	    // List<ImportIndexItem> History();

    }
}
