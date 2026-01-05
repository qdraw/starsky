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

	public CloudSyncController(ICloudSyncService cloudSyncService, CloudSyncSettings settings)
	{
		_cloudSyncService = cloudSyncService;
		_settings = settings;
	}

	/// <summary>
	/// Get current cloud sync status
	/// </summary>
	[HttpGet("status")]
	public IActionResult GetStatus()
	{
		return Ok(new
		{
			enabled = _settings.Enabled,
			provider = _settings.Provider,
			remoteFolder = _settings.RemoteFolder,
			syncFrequencyMinutes = _settings.SyncFrequencyMinutes,
			syncFrequencyHours = _settings.SyncFrequencyHours,
			deleteAfterImport = _settings.DeleteAfterImport,
			isSyncInProgress = _cloudSyncService.IsSyncInProgress,
			lastSyncResult = _cloudSyncService.LastSyncResult
		});
	}

	/// <summary>
	/// Trigger a manual sync
	/// </summary>
	[HttpPost("sync")]
	public async Task<IActionResult> TriggerSync()
	{
		if (!_settings.Enabled)
		{
			return BadRequest(new { message = "Cloud sync is disabled" });
		}

		if (_cloudSyncService.IsSyncInProgress)
		{
			return Conflict(new { message = "A sync operation is already in progress" });
		}

		var result = await _cloudSyncService.SyncAsync(CloudSyncTriggerType.Manual);
		return Ok(result);
	}

	/// <summary>
	/// Get the last sync result
	/// </summary>
	[HttpGet("last-result")]
	public IActionResult GetLastResult()
	{
		var lastResult = _cloudSyncService.LastSyncResult;
		if (lastResult == null)
		{
			return NotFound(new { message = "No sync has been performed yet" });
		}

		return Ok(lastResult);
	}
}

