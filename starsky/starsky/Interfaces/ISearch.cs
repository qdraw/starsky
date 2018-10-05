using starsky.ViewModels;

namespace starsky.Interfaces
{
    public interface ISearch
    {
        SearchViewModel Search(string query = "", int pageNumber = 0, bool enableCache = true);
    }
}
