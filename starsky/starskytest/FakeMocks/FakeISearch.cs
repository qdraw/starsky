using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.search.Interfaces;
using starsky.feature.search.ViewModels;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeISearch : ISearch
{
	public FakeISearch(List<FileIndexItem>? items = null)
	{
		Items = items ?? [];
	}

	private List<FileIndexItem> Items { get; }

	public string? LastQuery { get; private set; }
	public int LastPage { get; private set; }

	public Task<SearchViewModel> Search(string query = "", int pageNumber = 0,
		bool enableCache = true, bool collections = true)
	{
		LastQuery = query;
		LastPage = pageNumber;
		return Task.FromResult(new SearchViewModel
		{
			FileIndexItems = Items,
			SearchQuery = query,
			PageNumber = pageNumber,
			SearchCount = Items.Count
		});
	}

	public bool? RemoveCache(string query)
	{
		return true;
	}
}
