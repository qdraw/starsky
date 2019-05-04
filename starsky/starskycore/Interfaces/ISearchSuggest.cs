using System.Collections.Generic;

namespace starskycore.Interfaces
{
	
	public interface ISearchSuggest
	{
		IEnumerable<string> SearchSuggest(string query);
//		IEnumerable<KeyValuePair<string,int>> SearchSuggestPa(string query, int maxResults)

		Dictionary<string,int> Populate();
	}
}
