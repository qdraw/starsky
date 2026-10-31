using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     Background services have no request, so no tenant: they run their work once per active
///     tenant, each in its own scope with its own tenant context (never share a scoped service
///     between tenants)
/// </summary>
public static class TenantJobs
{
	/// <summary>
	///     A failing tenant does not stop the others; the exception goes to onError
	/// </summary>
	public static Task ForEachActiveTenantAsync(IServiceScopeFactory scopeFactory,
		ITenantRegistry registry, Func<IServiceProvider, string, Task> work,
		Action<string, Exception>? onError = null, CancellationToken cancellationToken = default)
	{
		return ForTenantsAsync(scopeFactory, System.Linq.Enumerable.ToArray(registry.ActiveTenantIds),
			work, onError, cancellationToken);
	}

	public static async Task ForTenantsAsync(IServiceScopeFactory scopeFactory,
		System.Collections.Generic.IEnumerable<string> tenantIds,
		Func<IServiceProvider, string, Task> work,
		Action<string, Exception>? onError = null, CancellationToken cancellationToken = default)
	{
		foreach ( var tenantId in tenantIds )
		{
			if ( cancellationToken.IsCancellationRequested )
			{
				return;
			}

			try
			{
				using var scope = scopeFactory.CreateTenantScope(tenantId);
				TenantAmbient.Set(tenantId);
				await work(scope.ServiceProvider, tenantId);
			}
			catch ( Exception exception ) when ( exception is not OperationCanceledException )
			{
				onError?.Invoke(tenantId, exception);
			}
			finally
			{
				// the next tenant (and the caller) must not inherit this tenant
				TenantAmbient.Reset();
			}
		}
	}
}
