using System.Collections.Generic;

namespace starskycore.Interfaces
{
	
	public interface ISearchSuggest
	{
		List<string> SearchSuggest(string query);
	}
}
