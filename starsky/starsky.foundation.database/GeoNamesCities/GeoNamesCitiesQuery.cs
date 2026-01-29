using System;
using System.Collections.Generic;
using System.Linq;
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
		catch ( ObjectDisposedException )
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

	private static async Task<GeoNameCity> AddItem(ApplicationDbContext context,
		GeoNameCity item)
	{
		context.GeoNameCities.Add(item);
		await context.SaveChangesAsync();
		context.Attach(item).State = EntityState.Detached;
		return item;
	}

	public async Task<List<GeoNameCity>> Search(string search,
		params string[] fields)
	{
		if ( string.IsNullOrWhiteSpace(search) || fields.Length == 0 )
		{
			return [];
		}

		var query = dbContext.GeoNameCities.AsNoTracking();

		var parameter = System.Linq.Expressions.Expression.Parameter(typeof(GeoNameCity), "x");
		System.Linq.Expressions.Expression? predicate = null;
		var searchToLower = search.ToLower();
		var searchExpr = System.Linq.Expressions.Expression.Constant(searchToLower, typeof(string));

		foreach ( var field in fields )
		{
			var property = typeof(GeoNameCity).GetProperty(field);
			if ( property == null || property.PropertyType != typeof(string) )
			{
				continue;
			}

			var propertyExpr = System.Linq.Expressions.Expression.Property(parameter, property);
			var notNullExpr = System.Linq.Expressions.Expression.NotEqual(propertyExpr,
				System.Linq.Expressions.Expression.Constant(null, typeof(string)));
			var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
			var propertyToLower =
				System.Linq.Expressions.Expression.Call(propertyExpr, toLowerMethod!);
			var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
			var containsExpr = System.Linq.Expressions.Expression.Call(propertyToLower,
				containsMethod!, searchExpr);
			var andExpr = System.Linq.Expressions.Expression.AndAlso(notNullExpr, containsExpr);
			predicate = predicate == null
				? andExpr
				: System.Linq.Expressions.Expression.OrElse(predicate, andExpr);
		}

		if ( predicate == null )
		{
			return [];
		}

		var lambda =
			System.Linq.Expressions.Expression
				.Lambda<Func<GeoNameCity, bool>>(predicate, parameter);
		return await query.Where(lambda).ToListAsync();
	}
}
