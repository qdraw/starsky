using starsky.ViewModels;

namespace starsky.Interfaces
{
    public interface ISearch
    {
        SearchViewModel Search(string query = "", int p = 0);
    }
}
