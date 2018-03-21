using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.Interfaces
{
    public interface ISync
    {
        IEnumerable<string> SyncFiles(string subPath = "/");
    }
}
