using System.Collections.Generic;

namespace starsky.Interfaces
{
    public interface IImport
    {
        List<string> Import(IEnumerable<string> inputFullPathList, bool deleteAfter = false, bool ageFileFilter = true, bool recursiveDirectory = false);
    }
}