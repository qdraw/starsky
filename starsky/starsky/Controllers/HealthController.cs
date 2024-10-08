using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Extensions;
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

	private readonly IMemoryCache? _cache;
	private readonly HealthCheckService _service;

	public HealthController(HealthCheckService service,
		IMemoryCache? memoryCache = null)
	{
		_service = service;
		_cache = memoryCache;
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
		var result = await CheckHealthAsyncWithTimeout(10000);
		if ( result.Status == HealthStatus.Healthy )
		{
			return Content(result.Status.ToString());
		}

		Response.StatusCode = 503;
		return Content(result.Status.ToString());
	}

	/// <summary>
	///     With timeout after 15 seconds
	/// </summary>
	/// <param name="timeoutTime">in milliseconds, defaults to 15 seconds</param>
	/// <returns>report</returns>
	internal async Task<HealthReport> CheckHealthAsyncWithTimeout(int timeoutTime = 15000)
	{
		const string healthControllerCacheKey = "health";
		try
		{
			if ( _cache != null &&
			     _cache.TryGetValue(healthControllerCacheKey, out var objectHealthStatus) &&
			     objectHealthStatus is HealthReport healthStatus &&
			     healthStatus.Status == HealthStatus.Healthy )
			{
				return healthStatus;
			}

			var result = await _service.CheckHealthAsync().TimeoutAfter(timeoutTime);
			if ( _cache != null && result.Status == HealthStatus.Healthy )
			{
				_cache.Set(healthControllerCacheKey, result, new TimeSpan(0, 1, 30));
			}

			return result;
		}
		catch ( TimeoutException exception )
		{
			var entry = new HealthReportEntry(
				HealthStatus.Unhealthy,
				"timeout",
				TimeSpan.FromMilliseconds(timeoutTime),
				exception,
				null);

			return new HealthReport(
				new Dictionary<string, HealthReportEntry> { { "timeout", entry } },
				TimeSpan.FromMilliseconds(timeoutTime));
		}
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
		var result = await CheckHealthAsyncWithTimeout();

		var health = CreateHealthEntryLog(result);
		if ( !health.IsHealthy )
		{
			Response.StatusCode = 503;
		}

		return Json(health);
	}

	private static HealthView CreateHealthEntryLog(HealthReport result)
	{
		var health = new HealthView
		{
			IsHealthy = result.Status == HealthStatus.Healthy,
			TotalDuration = result.TotalDuration
		};

		foreach ( var (key, value) in result.Entries )
		{
			health.Entries.Add(
				new HealthEntry
				{
					Duration = value.Duration,
					Name = key,
					IsHealthy = value.Status == HealthStatus.Healthy,
					Description = value.Description + value.Exception?.Message +
					              value.Exception?.StackTrace
				}
			);
		}

		return health;
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
