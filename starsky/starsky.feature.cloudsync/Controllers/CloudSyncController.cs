using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.cloudsync.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.cloudsync.Controllers;

[Authorize]
[ApiController]
[Route("api/cloudsync")]
public class CloudSyncController : ControllerBase
{
	private readonly ICloudSyncService _cloudSyncService;
	private readonly CloudSyncSettings _settings;

	public CloudSyncController(ICloudSyncService cloudSyncService, AppSettings appSettings)
	{
		_cloudSyncService = cloudSyncService;
		_settings = appSettings.CloudSync ?? new CloudSyncSettings();
	}

	/// <summary>
	///     Get current cloud sync status for all providers
	/// </summary>
	[HttpGet("status")]
	public IActionResult GetStatus()
	{
		return Ok(new
		{
			providers = _settings.Providers.Select(p => new
			{
				id = p.Id,
				enabled = p.Enabled,
				provider = p.Provider,
				remoteFolder = p.RemoteFolder,
				syncFrequencyMinutes = p.SyncFrequencyMinutes,
				syncFrequencyHours = p.SyncFrequencyHours,
				deleteAfterImport = p.DeleteAfterImport
			}),
			isSyncInProgress = _cloudSyncService.IsSyncInProgress,
			lastSyncResults = _cloudSyncService.LastSyncResults
		});
	}

	/// <summary>
	///     Get status for a specific provider
	/// </summary>
	[HttpGet("status/{providerId}")]
	public IActionResult GetProviderStatus(string providerId)
	{
		var provider = _settings.Providers.FirstOrDefault(p => p.Id == providerId);
		if ( provider == null )
		{
			return NotFound(new { message = $"Provider '{providerId}' not found" });
		}

		var lastResult = _cloudSyncService.LastSyncResults.TryGetValue(providerId, out var result)
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
	[HttpPost("sync")]
	public async Task<IActionResult> TriggerSyncAll()
	{
		if ( !_settings.Providers.Any(p => p.Enabled) )
		{
			return BadRequest(new { message = "No cloud sync providers are enabled" });
		}

		if ( _cloudSyncService.IsSyncInProgress )
		{
			return Conflict(new { message = "A sync operation is already in progress" });
		}

		var results = await _cloudSyncService.SyncAllAsync(CloudSyncTriggerType.Manual);
		return Ok(new { results });
	}

	/// <summary>
	///     Trigger a manual sync for a specific provider
	/// </summary>
	[HttpPost("sync/{providerId}")]
	public async Task<IActionResult> TriggerSync(string providerId)
	{
		var provider = _settings.Providers.FirstOrDefault(p => p.Id == providerId);
		if ( provider == null )
		{
			return NotFound(new { message = $"Provider '{providerId}' not found" });
		}

		if ( !provider.Enabled )
		{
			return BadRequest(new { message = $"Provider '{providerId}' is disabled" });
		}

		var result = await _cloudSyncService.SyncAsync(providerId, CloudSyncTriggerType.Manual);
		return Ok(result);
	}

	/// <summary>
	///     Get the last sync results for all providers
	/// </summary>
	[HttpGet("last-results")]
	public IActionResult GetLastResults()
	{
		var lastResults = _cloudSyncService.LastSyncResults;
		if ( !lastResults.Any() )
		{
			return NotFound(new { message = "No sync has been performed yet" });
		}

		return Ok(lastResults);
	}

	/// <summary>
	///     Get the last sync result for a specific provider
	/// </summary>
	[HttpGet("last-result/{providerId}")]
	public IActionResult GetLastResult(string providerId)
	{
		if ( _cloudSyncService.LastSyncResults.TryGetValue(providerId, out var result) )
		{
			return Ok(result);
		}

		return NotFound(new { message = $"No sync result found for provider '{providerId}'" });
	}
}
