using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.search.Interfaces;

namespace starskytest.FakeMocks;

public class FakeISearchSuggest : ISearchSuggest
{
	public FakeISearchSuggest(List<string>? suggestions = null)
	{
		Suggestions = suggestions ?? [];
	}

	private List<string> Suggestions { get; }

	public Task<IEnumerable<string>> SearchSuggest(string query, bool system)
	{
		return Task.FromResult(
			Suggestions.Where(p => p.StartsWith(query)));
	}

	public Task<List<KeyValuePair<string, int>>> GetAllSuggestions()
	{
		return Task.FromResult(Suggestions.Select(p => new KeyValuePair<string, int>(p, 1))
			.ToList());
	}

	public Task<List<KeyValuePair<string, int>>> Inflate()
	{
		return GetAllSuggestions();
	}
}
