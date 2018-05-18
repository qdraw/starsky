using System.Collections.Generic;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Interfaces
{
    public interface ISearch
    {
//        IEnumerable<FileIndexItem> SearchObjectItem(string tag = "", int pageNumber = 0);
//        int SearchLastPageNumber(string tag);
//        int SearchCount(string tag = "");
        SearchViewModel Search(string query = "", int p = 0);
    }
}
