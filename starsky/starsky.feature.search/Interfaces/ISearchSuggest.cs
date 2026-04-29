using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.feature.search.Interfaces;

public interface ISearchSuggest
{
	Task<IEnumerable<string>> SearchSuggest(string query, bool system);
	Task<IEnumerable<string>> SearchCameraSuggest(string query);
	Task<List<KeyValuePair<string, int>>> GetAllSuggestions();
	Task<List<KeyValuePair<string, int>>> Inflate();
}
