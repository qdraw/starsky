using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;

namespace starsky.Controllers;

[Authorize]
[ApiController]
[Route("api/mount-watcher")]
public sealed class MountWatcherController(ICameraMountWatcherService watcherService) : ControllerBase
{
	[HttpGet("status")]
	[ProducesResponseType(typeof(MountWatcherStatusModel), 200)]
	public IActionResult Status()
	{
		return Ok(watcherService.GetStatus());
	}

	[HttpPost("start")]
	[ProducesResponseType(typeof(MountWatcherStatusModel), 200)]
	public async Task<IActionResult> StartAsync()
	{
		return Ok(await watcherService.StartAsync());
	}

	[HttpPost("stop")]
	[ProducesResponseType(typeof(MountWatcherStatusModel), 200)]
	public async Task<IActionResult> StopAsync()
	{
		return Ok(await watcherService.StopAsync());
	}
}



