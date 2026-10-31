using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Tenancy;

[assembly: InternalsVisibleTo("starskytest"),
           InternalsVisibleTo("starsky.foundation.settings")]

namespace starsky.foundation.database.Query;

public class InjectServiceScope(
	IServiceScopeFactory scopeFactory,
	ITenantContext? tenantContext = null)
{

	internal TResult Execute<TResult>(Func<ApplicationDbContext, TResult> action)
	{
		using var scope = scopeFactory.CreateTenantScope(tenantContext);
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		return action(dbContext);
	}

	internal async Task<TResult> ExecuteAsync<TResult>(
		Func<ApplicationDbContext, Task<TResult>> action)
	{
		using var scope = scopeFactory.CreateTenantScope(tenantContext);
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		return await action(dbContext);
	}
}
