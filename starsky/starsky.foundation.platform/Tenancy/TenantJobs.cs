using System;
using System.Collections.Generic;
using System.Linq;
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
	///     A failing tenant does not stop the others: the failures are returned (and go to onError)
	/// </summary>
	public static Task<List<TenantFailure>> ForEachActiveTenantAsync(
		IServiceScopeFactory scopeFactory,
		ITenantRegistry registry, Func<IServiceProvider, string, Task> work,
		Action<string, Exception>? onError = null, CancellationToken cancellationToken = default)
	{
		return ForTenantsAsync(scopeFactory, registry.ActiveTenantIds.ToArray(),
			work, onError, cancellationToken);
	}

	public static async Task<List<TenantFailure>> ForTenantsAsync(
		IServiceScopeFactory scopeFactory,
		IEnumerable<string> tenantIds,
		Func<IServiceProvider, string, Task> work,
		Action<string, Exception>? onError = null, CancellationToken cancellationToken = default)
	{
		var failures = new List<TenantFailure>();
		foreach ( var tenantId in tenantIds )
		{
			if ( cancellationToken.IsCancellationRequested )
			{
				return failures;
			}

			try
			{
				using var scope = scopeFactory.CreateTenantScope(tenantId);
				TenantAmbient.Set(tenantId);
				await work(scope.ServiceProvider, tenantId);
			}
			catch ( Exception exception ) when ( exception is not OperationCanceledException )
			{
				failures.Add(new TenantFailure(tenantId, exception));
				onError?.Invoke(tenantId, exception);
			}
			finally
			{
				// the next tenant (and the caller) must not inherit this tenant
				TenantAmbient.Reset();
			}
		}

		return failures;
	}

	/// <summary>
	///     The results of the tenants that have one (null: nothing for this tenant)
	/// </summary>
	public static async Task<List<T>> SelectForTenantsAsync<T>(IServiceScopeFactory scopeFactory,
		IEnumerable<string> tenantIds, Func<IServiceProvider, string, Task<T?>> work,
		Action<string, Exception>? onError = null, CancellationToken cancellationToken = default)
		where T : class
	{
		var results = new List<T>();
		await ForTenantsAsync(scopeFactory, tenantIds, async (serviceProvider, tenantId) =>
		{
			var result = await work(serviceProvider, tenantId);
			if ( result != null )
			{
				results.Add(result);
			}
		}, onError, cancellationToken);
		return results;
	}
}

public sealed record TenantFailure(string TenantId, Exception Exception);
