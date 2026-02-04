using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Diagnostics.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;

namespace starsky.foundation.database.Diagnostics;

[Service(typeof(IDiagnosticsService), InjectionLifetime = InjectionLifetime.Scoped)]
public class DiagnosticsService(
	ApplicationDbContext dbContext,
	IServiceScopeFactory scopeFactory) : IDiagnosticsService
{
	public async Task<DiagnosticsItem?> GetItem(DiagnosticsType key)
	{
		async Task<DiagnosticsItem?> GetItemLocal(ApplicationDbContext context)
		{
			return await context.Diagnostics.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Key == Enum.GetName(key));
		}

		try
		{
			return await GetItemLocal(dbContext);
		}
		catch ( ObjectDisposedException )
		{
			var context = new InjectServiceScope(scopeFactory).Context();
			return await GetItemLocal(context);
		}
	}

	public async Task<DiagnosticsItem?> AddOrUpdateItem(DiagnosticsType key, string value)
	{
		return await AddOrUpdateItem(new DiagnosticsItem
		{
			Key = Enum.GetName(key) ?? string.Empty, Value = value, LastEdited = DateTime.UtcNow
		});
	}

	public async Task<DiagnosticsItem?> AddOrUpdateItem(DiagnosticsItem item)
	{
		if ( !Enum.TryParse(item.Key, out DiagnosticsType settingsType) )
		{
			return null;
		}

		var existingItem = ( await GetItem(settingsType) )?.Value;
		if ( string.IsNullOrEmpty(existingItem) )
		{
			try
			{
				return await AddItem(dbContext, item);
			}
			catch ( ObjectDisposedException )
			{
				var context = new InjectServiceScope(scopeFactory).Context();
				return await AddItem(context, item);
			}
		}

		try
		{
			return await UpdateItem(dbContext, item);
		}
		catch ( ObjectDisposedException )
		{
			var context = new InjectServiceScope(scopeFactory).Context();
			return await UpdateItem(context, item);
		}
	}

	private static async Task<DiagnosticsItem> UpdateItem(ApplicationDbContext context,
		DiagnosticsItem item)
	{
		context.Attach(item).State = EntityState.Modified;
		context.Diagnostics.Update(item);
		await context.SaveChangesAsync();
		context.Attach(item).State = EntityState.Detached;
		return item;
	}

	private static async Task<DiagnosticsItem> AddItem(ApplicationDbContext context,
		DiagnosticsItem item)
	{
		context.Diagnostics.Add(item);
		await context.SaveChangesAsync();
		context.Attach(item).State = EntityState.Detached;
		return item;
	}
}
