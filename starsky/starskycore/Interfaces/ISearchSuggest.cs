using System.Collections.Generic;

namespace starskycore.Interfaces
{
	
	public interface ISearchSuggest
	{
		IEnumerable<string> SearchSuggest(string query);
		IEnumerable<KeyValuePair<string, int>> GetAllSuggestions();
		List<KeyValuePair<string,int>> Inflate();
	}
}
