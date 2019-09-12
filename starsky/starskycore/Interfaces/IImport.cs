using System.Collections.Generic;
using starskycore.Models;

namespace starskycore.Interfaces
{
    public interface IImport
    {
        List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings);
	    List<ImportIndexItem> Preflight(List<string> inputFileFullPaths, ImportSettingsModel importSettings);
	    List<ImportIndexItem> History();

    }
}
