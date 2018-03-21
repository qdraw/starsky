using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface ISearch
    {
        IEnumerable<FileIndexItem> SearchObjectItem(string tag = "", int pageNumber = 0);
        int SearchLastPageNumber(string tag);
    }
}
