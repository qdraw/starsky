using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;

[assembly: InternalsVisibleTo("starskytest"),
           InternalsVisibleTo("starsky.foundation.settings")]

namespace starsky.foundation.database.Query;

public class InjectServiceScope
{
	private readonly IServiceScopeFactory? _scopeFactory;

	public InjectServiceScope(IServiceScopeFactory? scopeFactory)
	{
		if ( scopeFactory == null )
		{
			return;
		}

		_scopeFactory = scopeFactory;
	}

	/// <summary>
	///     Dependency injection, used in background tasks
	/// </summary>
	[Obsolete("Use ExecuteAsync instead to ensure proper disposal of the scope and DbContext.")]
	internal ApplicationDbContext Context()
	{
		if ( _scopeFactory == null )
		{
			return null!;
		}

		using var scope = _scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		return dbContext;
	}

	internal TResult Execute<TResult>(Func<ApplicationDbContext, TResult> action)
	{
		if ( _scopeFactory == null )
		{
			throw new InvalidOperationException("ScopeFactory is null");
		}

		using var scope = _scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		return action(dbContext);
	}

	internal async Task<TResult> ExecuteAsync<TResult>(
		Func<ApplicationDbContext, Task<TResult>> action)
	{
		if ( _scopeFactory == null )
		{
			throw new InvalidOperationException("ScopeFactory is null");
		}

		using var scope = _scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		return await action(dbContext);
	}
}
