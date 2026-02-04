using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Diagnostics;
using starsky.foundation.database.Diagnostics.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeDiagnosticsService : IDiagnosticsService
{
	private readonly Dictionary<string, DiagnosticsItem> _items = new();

	public void SetItem(DiagnosticsItem item)
	{
		_items[item.Key] = item;
	}
	
	public Task<DiagnosticsItem?> GetItem(DiagnosticsType key)
	{
		return Task.FromResult(_items.TryGetValue(key.ToString(), out var item) ? item : null);
	}

	public Task<DiagnosticsItem?> AddOrUpdateItem(DiagnosticsType key, string value)
	{
		_items[key.ToString()] = new DiagnosticsItem { Key = key.ToString(), Value = value };
		return Task.FromResult(_items[key.ToString()])!;
	}
}
