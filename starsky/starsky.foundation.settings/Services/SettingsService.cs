using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Formats;
using starsky.foundation.settings.Interfaces;

namespace starsky.foundation.settings.Services;

[Service(typeof(ISettingsService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class SettingsService : ISettingsService
{
	private readonly ApplicationDbContext _context;
	private readonly IServiceScopeFactory _scopeFactory;

	public SettingsService(ApplicationDbContext dbContext, IServiceScopeFactory scopeFactory)
	{
		_context = dbContext;
		_scopeFactory = scopeFactory;
	}

	public async Task<SettingsItem?> GetSetting(SettingsType key)
	{
		async Task<SettingsItem?> GetSettingLocal(ApplicationDbContext context)
		{
			return await context.Settings.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Key == Enum.GetName(key));
		}

		try
		{
			return await GetSettingLocal(_context);
		}
		catch ( ObjectDisposedException )
		{
			var context = new InjectServiceScope(_scopeFactory).Context();
			return await GetSettingLocal(context);
		}
	}

	public async Task<T?> GetSetting<T>(SettingsType key)
	{
		var data = await GetSetting(key);
		return CastSetting<T>(data);
	}

	public static T? CastSetting<T>(SettingsItem? data)
	{
		if ( data?.Value == null ) return default;

		if ( typeof(T) == typeof(DateTime) && DateTime.TryParseExact(data.Value,
				SettingsFormats.LastSyncBackgroundDateTime,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal,
				out var expectDateTime) )
		{
			return ( T )( object )expectDateTime;
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
			Key = Enum.GetName(key) ?? string.Empty,
			Value = value
		});
	}

	public async Task<SettingsItem?> AddOrUpdateSetting(SettingsItem item)
	{
		if ( !Enum.TryParse(item.Key, out SettingsType settingsType) )
		{
			return null;
		}

		var existingItem = ( await GetSetting(settingsType) )?.Value;
		if ( string.IsNullOrEmpty(existingItem) )
		{

			try
			{
				return await AddItem(_context, item);
			}
			catch ( ObjectDisposedException )
			{
				var context = new InjectServiceScope(_scopeFactory).Context();
				return await AddItem(context, item);
			}
		}

		try
		{
			return await UpdateItem(_context, item);
		}
		catch ( ObjectDisposedException )
		{
			var context = new InjectServiceScope(_scopeFactory).Context();
			return await UpdateItem(context, item);
		}
	}

	private static async Task<SettingsItem> AddItem(ApplicationDbContext context, SettingsItem item)
	{
		context.Settings.Add(item);
		await context.SaveChangesAsync();
		context.Attach(item).State = EntityState.Detached;
		return item;
	}

	private static async Task<SettingsItem> UpdateItem(ApplicationDbContext context, SettingsItem item)
	{
		context.Attach(item).State = EntityState.Modified;
		context.Settings.Update(item);
		await context.SaveChangesAsync();
		context.Attach(item).State = EntityState.Detached;
		return item;
	}
}
