#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.realtime.Enums;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.realtime.Services;

namespace starskytest.FakeMocks;

public class FakeISettingsService : ISettingsService
{
	public List<SettingsItem> Items { get; set; } = new();
	
	public Task<SettingsItem?> GetSetting(SettingsType key)
	{
		var result =  Items.FirstOrDefault(p => p.Key == Enum.GetName(key));
		return Task.FromResult(result);
	}

	public Task<T?> GetSetting<T>(SettingsType key)
	{
		var result =  Items.FirstOrDefault(p => p.Key == Enum.GetName(key));
		var result2 =   SettingsService.CastSetting<T>(result);
		return Task.FromResult(result2);
	}

	public Task<SettingsItem?> AddOrUpdateSetting(SettingsItem item)
	{
		if ( item.Key == null || !Enum.TryParse(item.Key, out SettingsType _) )
		{
			return Task.FromResult(null as SettingsItem);
		}
	
		var existingItem =  Items.FindIndex(p => p.Key == item.Key);
		if (existingItem == -1)
		{
			Items.Add(item);
			return Task.FromResult(item)!;
		}

		Items[existingItem] = item;
		return Task.FromResult(item)!;
	}

	public async Task<SettingsItem?> AddOrUpdateSetting(SettingsType key, string value)
	{
		return await AddOrUpdateSetting(new SettingsItem
		{
			Key = Enum.GetName(key), Value = value
		});
	}
}
