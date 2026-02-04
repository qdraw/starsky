using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.feature.health.HealthCheck.Interfaces;
using starsky.foundation.platform.VersionHelpers;
using starsky.project.web.ViewModels;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.Controllers;

public sealed class HealthController : Controller
{
	/// <summary>
	///     Check if min version is matching
	///     keywords: MinVersion or Version( or SemVersion(
	/// </summary>
	internal const string MinimumVersion = "0.5"; // only insert 0.5 or 0.6

	/// <summary>
	///     Name of the header for api version
	/// </summary>
	private const string ApiVersionHeaderName = "x-api-version";

	private readonly ICheckHealthService _checkHealthService;

	public HealthController(ICheckHealthService checkHealthService)
	{
		_checkHealthService = checkHealthService;
	}

	/// <summary>
	///     Check if the service has any known errors and return only a string
	///     Public API
	/// </summary>
	/// <returns></returns>
	/// <response code="200">Ok</response>
	/// <response code="503">503 Service Unavailable</response>
	[HttpGet("/api/health")]
	[HttpHead("/api/health")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(string), 200)]
	[ProducesResponseType(typeof(string), 503)]
	[AllowAnonymous]
	public async Task<IActionResult> Index()
	{
		var result = await _checkHealthService.CheckHealthWithTimeoutAsync(10000);
		if ( result.Status == HealthStatus.Healthy )
		{
			return Content(result.Status.ToString());
		}

		Response.StatusCode = 503;
		return Content(result.Status.ToString());
	}


	/// <summary>
	///     Check if the service has any known errors
	///     For Authorized Users only
	/// </summary>
	/// <returns></returns>
	/// <response code="200">Ok</response>
	/// <response code="503">503 Service Unavailable</response>
	/// <response code="401">Login first</response>
	[HttpGet("/api/health/details")]
	[Authorize] // <--------------
	[Produces("application/json")]
	[ProducesResponseType(typeof(HealthView), 200)]
	[ProducesResponseType(typeof(HealthView), 503)]
	[ProducesResponseType(401)]
	public async Task<IActionResult> Details()
	{
		var result = await _checkHealthService.CheckHealthWithTimeoutAsync();

		var health = _checkHealthService.CreateHealthEntryLog(result);
		if ( !health.IsHealthy )
		{
			Response.StatusCode = 503;
		}

		return Json(health);
	}

	/// <summary>
	///     Check if Client/App version has a match with the API-version
	///     the parameter 'version' is checked first, and if missing the x-api-version header is used
	/// </summary>
	/// <returns>status</returns>
	/// <response code="200">Ok</response>
	/// <response code="202">Version mismatch</response>
	/// <response code="400">Missing x-api-version header OR bad formatted version in header</response>
	[HttpPost("/api/health/version")]
	[AllowAnonymous]
	public IActionResult Version(string? version = null)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		if ( string.IsNullOrEmpty(version) )
		{
			var headerVersion =
				Request.Headers.FirstOrDefault(p =>
					p.Key == ApiVersionHeaderName).Value;
			if ( !string.IsNullOrEmpty(headerVersion) )
			{
				version = headerVersion;
			}
		}

		if ( string.IsNullOrEmpty(version) )
		{
			return BadRequest(
				$"Missing version data, Add {ApiVersionHeaderName} header with value");
		}

		try
		{
			if ( SemVersion.Parse(version) >= SemVersion.Parse(MinimumVersion) )
			{
				return Ok(version);
			}

			return StatusCode(StatusCodes.Status202Accepted,
				$"Please Upgrade ClientApp to {MinimumVersion} or newer");
		}
		catch ( ArgumentException )
		{
			return StatusCode(StatusCodes.Status400BadRequest,
				$"Parsing failed {version}");
		}
	}
}
