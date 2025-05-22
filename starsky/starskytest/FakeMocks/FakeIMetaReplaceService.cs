using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Services;

namespace starskytest.FakeMocks;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class FakeIMetaReplaceServiceData
{
	public string f { get; set; } = string.Empty;
	public string fieldName { get; set; } = string.Empty;
	public string search { get; set; } = string.Empty;
	public string replace { get; set; } = string.Empty;
}

public class FakeIMetaReplaceService : IMetaReplaceService
{
	private readonly List<FileIndexItem> _input = new();

	public FakeIMetaReplaceService(List<FileIndexItem>? input = null)
	{
		if ( input != null )
		{
			_input = input;
		}
	}

	[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
	public List<FakeIMetaReplaceServiceData> Data { get; set; } = new();

	public Task<List<FileIndexItem>> Replace(string f, string fieldName, string search,
		string? replace,
		bool collections)
	{
		var replaceItem = replace ?? string.Empty;
		Data.Add(new FakeIMetaReplaceServiceData
		{
			f = f, fieldName = fieldName, search = search, replace = replaceItem
		});

		return Task.FromResult(MetaReplaceService.SearchAndReplace(
			_input.Where(p => p.FilePath == f).ToList(), fieldName, search,
			replaceItem));
	}
}
