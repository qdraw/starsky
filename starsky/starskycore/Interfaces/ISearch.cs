using starsky.ViewModels;
using starskycore.ViewModels;

namespace starskycore.Interfaces
{
    public interface ISearch
    {
        SearchViewModel Search(string query = "", int pageNumber = 0, bool enableCache = true);
    }
}
