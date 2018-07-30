using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IImport
    {
        List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings);
    }
}