using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.platform.Tenancy;

public static class TenantScopeExtensions
{
	/// <summary>
	///     A new scope has a new TenantContext (the default tenant). Code that opens a scope on
	///     behalf of a request or job must carry the tenant over, or it silently reads another
	///     tenant's data
	/// </summary>
	public static IServiceScope CreateTenantScope(this IServiceScopeFactory scopeFactory,
		ITenantContext? tenantContext)
	{
		var scope = scopeFactory.CreateScope();
		if ( tenantContext != null &&
		     scope.ServiceProvider.GetService<ITenantContext>() is TenantContext newContext )
		{
			newContext.SetTenant(tenantContext.TenantId);
		}

		return scope;
	}
}
