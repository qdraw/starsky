using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;

[assembly: InternalsVisibleTo("starskytest"),
           InternalsVisibleTo("starsky.foundation.settings")]

namespace starsky.foundation.database.Query;

public class InjectServiceScope(IServiceScopeFactory scopeFactory)
{
	/// <summary>
	///     Dependency injection, used in background tasks
	/// </summary>
	[Obsolete("Use ExecuteAsync instead to ensure proper disposal of the scope and DbContext.")]
	internal ApplicationDbContext Context()
	{
		if ( scopeFactory == null )
		{
			return null!;
		}

		using var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		return dbContext;
	}

	internal TResult Execute<TResult>(Func<ApplicationDbContext, TResult> action)
	{
		using var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		return action(dbContext);
	}

	internal async Task<TResult> ExecuteAsync<TResult>(
		Func<ApplicationDbContext, Task<TResult>> action)
	{
		using var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		return await action(dbContext);
	}
}
