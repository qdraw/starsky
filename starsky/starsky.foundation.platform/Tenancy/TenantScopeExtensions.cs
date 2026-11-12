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
		return scopeFactory.CreateTenantScope(tenantContext?.TenantId);
	}

	/// <summary>
	///     Null is the default tenant
	/// </summary>
	public static IServiceScope CreateTenantScope(this IServiceScopeFactory scopeFactory,
		string? tenantId)
	{
		var scope = scopeFactory.CreateScope();
		if ( tenantId != null &&
		     scope.ServiceProvider.GetService<ITenantContext>() is TenantContext newContext )
		{
			// not the ambient tenant: the caller may open a scope for another tenant than its own
			newContext.SetTenant(tenantId, false);
		}

		return scope;
	}
}
