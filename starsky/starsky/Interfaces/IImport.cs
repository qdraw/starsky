using System.Collections.Generic;

namespace starsky.Interfaces
{
    public interface IImport
    {
        List<string> Import(string inputFullPath, bool deleteAfter = false, bool ageFileFilter = true);
    }
}