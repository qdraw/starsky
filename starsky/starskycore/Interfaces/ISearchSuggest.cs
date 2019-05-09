using System.Collections.Generic;

namespace starskycore.Interfaces
{
	
	public interface ISearchSuggest
	{
		IEnumerable<string> SearchSuggest(string query);

		List<KeyValuePair<string,int>> Inflate();
	}
}
