using System.Collections.Generic;

namespace starsky.Interfaces
{
    public interface ISync
    {
        IEnumerable<string> SyncFiles(string subPath = "/");
    }
}
