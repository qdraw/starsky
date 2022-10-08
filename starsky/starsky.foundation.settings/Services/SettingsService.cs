using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.realtime.Enums;
using starsky.foundation.realtime.Formats;
using starsky.foundation.realtime.Interfaces;

namespace starsky.foundation.realtime.Services;

[Service(typeof(ISettingsService), InjectionLifetime = InjectionLifetime.Scoped)]
public class SettingsService : ISettingsService
{
	private readonly ApplicationDbContext _context;

	public SettingsService(ApplicationDbContext dbContext)
	{
		_context = dbContext;
	}

	public async Task<SettingsItem?> GetSetting(SettingsType key)
	{
		return await _context.Settings.AsNoTracking().FirstOrDefaultAsync(p => p.Key == Enum.GetName(key));
	}

	public async Task<T?> GetSetting<T>(SettingsType key)
	{
		var data = await GetSetting(key);

		if ( data?.Value != null && typeof(T) == typeof(DateTime) && DateTime.TryParseExact(data.Value, 
			    SettingsFormats.LastSyncBackgroundDateTime, 
			    CultureInfo.InvariantCulture, 
			    DateTimeStyles.AssumeUniversal, 
			    out var expectDateTime) )
		{
			return (T)(object) expectDateTime;
		}

		try
		{
			return ( T? )( object? )data?.Value;
		}
		catch ( NullReferenceException )
		{
			return default;
		}
		catch ( InvalidCastException )
		{
			return default;
		}
	}

	public async Task<SettingsItem?> AddOrUpdateSetting(SettingsType key, string value)
	{
		return await AddOrUpdateSetting(new SettingsItem
		{
			Key = Enum.GetName(key), Value = value
		});
	}

	public async Task<SettingsItem?> AddOrUpdateSetting(SettingsItem item)
	{
		if ( !Enum.TryParse(item.Key, out SettingsType settingsType) )
		{
			return null;
		}

		var existingItem = ( await GetSetting(settingsType) )?.Value;
		if (string.IsNullOrEmpty(existingItem))
		{
			_context.Settings.Add(item);
			await _context.SaveChangesAsync();
			return item;
		}

		_context.Settings.Update(item);
		await _context.SaveChangesAsync();
		return item;
	}
}
