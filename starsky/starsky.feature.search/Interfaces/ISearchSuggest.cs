using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.search.Interfaces
{
	
	public interface ISearchSuggest
	{
		Task<IEnumerable<string>> SearchSuggest(string query);
		Task<IEnumerable<KeyValuePair<string, int>>> GetAllSuggestions();
		Task<List<KeyValuePair<string, int>>> Inflate();
	}
}
