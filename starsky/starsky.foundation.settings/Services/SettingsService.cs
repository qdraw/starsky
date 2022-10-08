using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.realtime.Formats;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Interfaces;

namespace starsky.foundation.settings.Services;

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
		return CastSetting<T>(data);
	}

	public static T? CastSetting<T>(SettingsItem? data)
	{
		if ( data?.Value == null) return default;
		
		if (typeof(T) == typeof(DateTime) && DateTime.TryParseExact(data.Value, 
			    SettingsFormats.LastSyncBackgroundDateTime, 
			    CultureInfo.InvariantCulture, 
			    DateTimeStyles.AssumeUniversal, 
			    out var expectDateTime) )
		{
			return (T)(object) expectDateTime;
		}

		try
		{
			return ( T? )( object? )data.Value;
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
			_context.Attach(item).State = EntityState.Detached;
			return item;
		}
		
		_context.Attach(item).State = EntityState.Modified;
		_context.Settings.Update(item);
		await _context.SaveChangesAsync();
		_context.Attach(item).State = EntityState.Detached;
		return item;
	}
}
