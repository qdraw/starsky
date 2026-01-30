using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.injection;

namespace starsky.foundation.database.GeoNamesCities;

[Service(typeof(IGeoNamesCitiesQuery), InjectionLifetime = InjectionLifetime.Scoped)]
public class GeoNamesCitiesQuery(
	ApplicationDbContext dbContext,
	IServiceScopeFactory scopeFactory) : IGeoNamesCitiesQuery
{
	public async Task<GeoNameCity?> GetItem(int geoNameId)
	{
		async Task<GeoNameCity?> GetItemLocal(ApplicationDbContext context)
		{
			return await context.GeoNameCities.AsNoTracking()
				.FirstOrDefaultAsync(p => p.GeonameId == geoNameId);
		}

		try
		{
			return await GetItemLocal(dbContext);
		}
		// InvalidOperationException can also be disposed (ObjectDisposedException)
		catch ( InvalidOperationException )
		{
			var context = new InjectServiceScope(scopeFactory).Context();
			return await GetItemLocal(context);
		}
	}

	public async Task<GeoNameCity> AddItem(GeoNameCity item)
	{
		if ( await GetItem(item.GeonameId) != null )
		{
			return item;
		}

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


	public async Task<List<GeoNameCity>> Search(string search, int maxResults,
		params string[] fields)
	{
		if ( string.IsNullOrWhiteSpace(search) || fields.Length == 0 )
		{
			return [];
		}

		try
		{
			return await SearchGetPredicate(dbContext, search, maxResults, fields);
		}
		// InvalidOperationException can also be disposed (ObjectDisposedException)
		catch ( InvalidOperationException )
		{
			var context = new InjectServiceScope(scopeFactory).Context();
			return await SearchGetPredicate(context, search, maxResults, fields);
		}
	}

	public async Task<List<GeoNameCity>> AddRange(List<GeoNameCity> items)
	{
		try
		{
			return await AddRange(dbContext, items);
		}
		catch ( ObjectDisposedException )
		{
			var context = new InjectServiceScope(scopeFactory).Context();
			return await AddRange(context, items);
		}
	}

	private static async Task<List<GeoNameCity>> AddRange(ApplicationDbContext context,
		List<GeoNameCity> items)
	{
		context.GeoNameCities.AddRange(items);
		await context.SaveChangesAsync();
		foreach ( var item in items )
		{
			context.Attach(item).State = EntityState.Detached;
		}

		return items;
	}

	private static async Task<GeoNameCity> AddItem(ApplicationDbContext context,
		GeoNameCity item)
	{
		context.GeoNameCities.Add(item);
		await context.SaveChangesAsync();
		context.Attach(item).State = EntityState.Detached;
		return item;
	}

	[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
	private static async Task<List<GeoNameCity>> SearchGetPredicate(ApplicationDbContext context,
		string search,
		int maxResults,
		string[] fields)
	{
		var query = context.GeoNameCities.AsNoTracking();

		var parameter = Expression.Parameter(typeof(GeoNameCity), "x");
		Expression? predicate = null;
		var searchToLower = search.ToLower();
		var searchExpr = Expression.Constant(searchToLower, typeof(string));

		foreach ( var field in fields )
		{
			var property = typeof(GeoNameCity).GetProperty(field);
			if ( property == null || property.PropertyType != typeof(string) )
			{
				continue;
			}

			var propertyExpr = Expression.Property(parameter, property);
			var notNullExpr = Expression.NotEqual(propertyExpr,
				Expression.Constant(null, typeof(string)));
			var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
			var propertyToLower =
				Expression.Call(propertyExpr, toLowerMethod!);
			var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
			var containsExpr = Expression.Call(propertyToLower,
				containsMethod!, searchExpr);
			var andExpr = Expression.AndAlso(notNullExpr, containsExpr);
			predicate = predicate == null
				? andExpr
				: Expression.OrElse(predicate, andExpr);
		}

		if ( predicate == null )
		{
			return [];
		}

		var lambda =
			Expression
				.Lambda<Func<GeoNameCity, bool>>(predicate, parameter);
		return await query.Where(lambda).OrderByDescending(p => p.Population).Take(maxResults)
			.ToListAsync();
	}
}
