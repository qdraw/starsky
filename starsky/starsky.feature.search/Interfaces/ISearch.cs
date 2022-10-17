using starsky.feature.search.ViewModels;

namespace starsky.feature.search.Interfaces
{
    public interface ISearch
    {
        SearchViewModel Search(string query = "", int pageNumber = 0, bool enableCache = true);
        bool? RemoveCache(string query);
    }
}
