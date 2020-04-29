using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starskycore.Models;

namespace starsky.feature.import.Interfaces
{
    public interface IImport
    {
	    /// <summary>
	    /// Test if file can be imported
	    /// </summary>
	    /// <param name="fullFilePathsList">list of paths</param>
	    /// <param name="importSettings">settings</param>
	    /// <returns></returns>
	    Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList,
		    ImportSettingsModel importSettings);

	    /// <summary>
	    /// Run Import 
	    /// </summary>
	    /// <param name="inputFullPathList">list of paths</param>
	    /// <param name="importSettings">settings</param>
	    /// <returns></returns>
	    Task<List<ImportIndexItem>> Importer(IEnumerable<string> inputFullPathList,
		    ImportSettingsModel importSettings);

    }
}
