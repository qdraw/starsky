using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.cloudimport.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;

namespace starsky.feature.cloudimport.Controllers;

[Authorize]
[ApiController]
[Route("api/cloud-import")]
public class CloudImportController(
	ICloudImportService cloudImportService,
	AppSettings appSettings,
	ITenantContext? tenantContext = null)
	: ControllerBase
{
	private readonly CloudImportSettings _settings =
		appSettings.CloudImport ?? new CloudImportSettings();

	// a provider imports into one tenant: the other tenants do not see it, or its results
	private string TenantId => tenantContext?.TenantId ?? TenantIdHelper.DefaultTenantId;

	// a controller lives for one request: compute once
	private List<CloudImportProviderSettings>? _providersOfTenant;

	private List<CloudImportProviderSettings> ProvidersOfTenant =>
		_providersOfTenant ??= _settings.GetProvidersOfTenant(TenantId);

	private Dictionary<string, CloudImportResult>? _lastResultsOfTenant;

	private Dictionary<string, CloudImportResult> LastResultsOfTenant =>
		_lastResultsOfTenant ??= GetLastResultsOfTenant();

	private Dictionary<string, CloudImportResult> GetLastResultsOfTenant()
	{
		var ids = ProvidersOfTenant.Select(p => p.Id).ToHashSet();
		return cloudImportService.LastSyncResults
			.Where(p => ids.Contains(p.Key))
			.ToDictionary(p => p.Key, p => p.Value);
	}

	/// <summary>
	///     Get current Cloud Import status for all providers
	/// </summary>
	/// <returns>get status</returns>
	[HttpGet("status")]
	public IActionResult GetStatus()
	{
		return Ok(new
		{
			providers = ProvidersOfTenant.Select(p => new
			{
				id = p.Id,
				enabled = p.Enabled,
				provider = p.Provider,
				remoteFolder = p.RemoteFolder,
				syncFrequencyMinutes = p.SyncFrequencyMinutes,
				syncFrequencyHours = p.SyncFrequencyHours,
				deleteAfterImport = p.DeleteAfterImport
			}),
			isSyncInProgress = cloudImportService.IsSyncInProgressForTenant(TenantId),
			lastSyncResults = LastResultsOfTenant
		});
	}


	/// <summary>
	///     Get status for a specific provider
	/// </summary>
	/// <param name="providerId">id of provider</param>
	/// <returns>status</returns>
	[HttpGet("status/{providerId}")]
	public IActionResult GetProviderStatus(string providerId)
	{
		var provider = ProvidersOfTenant.FirstOrDefault(p => p.Id == providerId);
		if ( provider == null )
		{
			return NotFound(new { message = $"Provider '{providerId}' not found" });
		}

		var lastResult = LastResultsOfTenant.TryGetValue(providerId, out var result)
			? result
			: null;

		return Ok(new
		{
			id = provider.Id,
			enabled = provider.Enabled,
			provider = provider.Provider,
			remoteFolder = provider.RemoteFolder,
			syncFrequencyMinutes = provider.SyncFrequencyMinutes,
			syncFrequencyHours = provider.SyncFrequencyHours,
			deleteAfterImport = provider.DeleteAfterImport,
			lastSyncResult = lastResult
		});
	}


	/// <summary>
	///     Trigger a manual sync for all enabled providers
	/// </summary>
	/// <returns>result of sync</returns>
	[HttpPost("sync")]
	public async Task<IActionResult> TriggerSyncAll()
	{
		if ( !ProvidersOfTenant.Exists(p => p.Enabled) )
		{
			return BadRequest(new { message = "No Cloud Import providers are enabled" });
		}

		if ( cloudImportService.IsSyncInProgressForTenant(TenantId) )
		{
			return Conflict(new { message = "A sync operation is already in progress" });
		}

		var results = await cloudImportService.SyncAllAsync(CloudImportTriggerType.Manual, TenantId);
		return Ok(new { results });
	}

	/// <summary>
	///     Trigger a manual sync for a specific provider
	/// </summary>
	/// <param name="providerId">which provider</param>
	/// <returns>status</returns>
	[HttpPost("sync/{providerId}")]
	public async Task<IActionResult> TriggerSync(string providerId)
	{
		var provider = ProvidersOfTenant.FirstOrDefault(p => p.Id == providerId);
		if ( provider == null )
		{
			return NotFound(new { message = $"Provider '{providerId}' not found" });
		}

		if ( !provider.Enabled )
		{
			return BadRequest(new { message = $"Provider '{provider.Id}' is disabled" });
		}

		if ( cloudImportService.IsSyncInProgressForTenant(TenantId) )
		{
			return Conflict(new { message = "A sync operation is already in progress" });
		}

		var result = await cloudImportService.SyncAsync(provider.Id, CloudImportTriggerType.Manual);
		return Ok(result);
	}

	/// <summary>
	///     Get the last sync results for all providers
	/// </summary>
	/// <returns>status</returns>
	[HttpGet("last-results")]
	public IActionResult GetLastResults()
	{
		var lastResults = LastResultsOfTenant;
		if ( lastResults.Count == 0 )
		{
			return NotFound(new { message = "No sync has been performed yet" });
		}

		return Ok(lastResults);
	}

	/// <summary>
	///     Get the last sync result for a specific provider
	/// </summary>
	/// <param name="providerId">which provider</param>
	/// <returns>status</returns>
	[HttpGet("last-result/{providerId}")]
	public IActionResult GetLastResult(string providerId)
	{
		if ( LastResultsOfTenant.TryGetValue(providerId, out var result) )
		{
			return Ok(result);
		}

		return NotFound(new { message = $"No sync result found for provider '{providerId}'" });
	}
}
