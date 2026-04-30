using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Helpers;

public static class QueueJobTenantEnforcer
{
	public static async Task<IReadOnlyList<BackgroundTaskQueueJob>> ExpandForTenantCoverageAsync(
		BackgroundTaskQueueJob job,
		IServiceProvider services,
		IWebLogger? logger,
		string queueName)
	{
		ArgumentNullException.ThrowIfNull(job);

		if ( job.TenantId.HasValue && !string.IsNullOrWhiteSpace(job.TenantSlug) )
		{
			return [ job ];
		}

		logger?.LogError(
			$"[{queueName}] Background job producer omitted tenant metadata for job type '{job.JobType ?? "(null)"}'. Expanding job to all enabled tenants.");

		var tenantId = job.TenantId;
		var tenantSlug = job.TenantSlug;

		var tenantContext = services.GetService<ITenantContext>();
		tenantId ??= tenantContext?.TenantId;
		tenantSlug ??= tenantContext?.TenantSlug;

		if ( tenantId.HasValue && !string.IsNullOrWhiteSpace(tenantSlug) )
		{
			return [ Clone(job, tenantId.Value, tenantSlug) ];
		}

		var db = services.GetService<ApplicationDbContext>();
		if ( db == null )
		{
			logger?.LogError(
				$"[{queueName}] Rejecting background job '{job.JobType ?? "(null)"}' because tenant metadata is missing and ApplicationDbContext is unavailable for tenant expansion.");
			throw new InvalidOperationException("TenantId and TenantSlug are required for background jobs");
		}

		var enabledTenants = await db.Tenants.AsNoTracking()
			.Where(t => t.IsEnabled)
			.Select(t => new { t.Id, t.Slug })
			.ToListAsync();

		if ( enabledTenants.Count == 0 )
		{
			logger?.LogError(
				$"[{queueName}] Rejecting background job '{job.JobType ?? "(null)"}' because no enabled tenants exist for expansion.");
			throw new InvalidOperationException("TenantId and TenantSlug are required for background jobs");
		}

		if ( enabledTenants.Count > 1 )
		{
			logger?.LogInformation(
				$"[{queueName}] Expanding job '{job.JobType ?? "(null)"}' to {enabledTenants.Count} tenants.");
		}

		return enabledTenants
			.Select(t => Clone(job, t.Id, t.Slug))
			.ToList();
	}

	private static BackgroundTaskQueueJob Clone(BackgroundTaskQueueJob job, int tenantId,
		string tenantSlug)
	{
		return new BackgroundTaskQueueJob
		{
			JobId = job.JobId,
			MetaData = job.MetaData,
			TraceParentId = job.TraceParentId,
			TenantId = tenantId,
			TenantSlug = tenantSlug,
			PriorityLane = job.PriorityLane,
			CreatedAtUtc = job.CreatedAtUtc,
			JobType = job.JobType,
			PayloadJson = job.PayloadJson
		};
	}

	public static void ValidateTenantOrThrow(BackgroundTaskQueueJob job, IWebLogger? logger,
		string queueName)
	{
		ArgumentNullException.ThrowIfNull(job);
		if ( job.TenantId.HasValue && !string.IsNullOrWhiteSpace(job.TenantSlug) )
		{
			return;
		}

		logger?.LogError(
			$"[{queueName}] Rejecting background job '{job.JobType ?? "(null)"}' because TenantId/TenantSlug is missing.");
		throw new InvalidOperationException("TenantId and TenantSlug are required for background jobs");
	}
}



