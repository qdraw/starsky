using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Exceptions;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.VersionHelpers;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.webtelemetry.Interfaces;
using starskycore.Helpers;
using starskycore.ViewModels;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.Controllers
{
	public class HealthController: Controller
	{
		private readonly HealthCheckService _service;
		private readonly ApplicationInsightsJsHelper _applicationInsightsJsHelper;
		private readonly ITelemetryService _telemetryService;

		public HealthController(HealthCheckService service, ITelemetryService telemetryService, 
			ApplicationInsightsJsHelper applicationInsightsJsHelper = null)
		{
			_service = service;
			_applicationInsightsJsHelper = applicationInsightsJsHelper;
			_telemetryService = telemetryService;
		}

		/// <summary>
		/// Check if the service has any known errors and return only a string
		/// Public API
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Ok</response>
		/// <response code="503">503 Service Unavailable</response>
		[HttpGet("/api/health")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(string), 200)]
		[ProducesResponseType(typeof(string), 503)]
		public async Task<IActionResult> Index()
		{
			var result = await _service.CheckHealthAsync().TimeoutAfter(15);
			if ( result.Status == HealthStatus.Healthy ) return Content(result.Status.ToString());

			Response.StatusCode = 503;
			PushNonHealthResultsToTelemetry(result);
			return Content(result.Status.ToString());
		}

		/// <summary>
		/// Push Non Healthy results to Telemetry Service
		/// </summary>
		/// <param name="result">report</param>
		private void PushNonHealthResultsToTelemetry(HealthReport result)
		{
			if ( result.Status == HealthStatus.Healthy ) return;
			var message = JsonSerializer.Serialize(CreateHealthEntryLog(result).Entries.Where(p => !p.IsHealthy));
			_telemetryService.TrackException(
				new TelemetryServiceException(message)
			);
		}

		/// <summary>
		/// Check if the service has any known errors
		/// For Authorized Users only
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Ok</response>
		/// <response code="503">503 Service Unavailable</response>
		/// <response code="401">Login first</response>
		[HttpGet("/api/health/details")]
		[Authorize] // <--------------
		[Produces("application/json")]
		[ProducesResponseType(typeof(HealthView),200)]
		[ProducesResponseType(typeof(HealthView),503)]
		[ProducesResponseType(401)]
		public async Task<IActionResult> Details()
		{
			var result = await _service.CheckHealthAsync();
			PushNonHealthResultsToTelemetry(result);

			var health = CreateHealthEntryLog(result);
			if ( !health.IsHealthy ) Response.StatusCode = 503;
			
			return Json(health);
		}

		private HealthView CreateHealthEntryLog(HealthReport result)
		{
			var health = new HealthView
			{
				IsHealthy = result.Status == HealthStatus.Healthy,
				TotalDuration = result.TotalDuration
			};
			
			foreach ( var (key, value) in result.Entries )
			{
				health.Entries.Add(
					new HealthEntry{
							Duration = value.Duration, 
							Name = key, 
							IsHealthy = value.Status == HealthStatus.Healthy,
							Description = value.Description + value.Exception?.Message + value.Exception?.StackTrace
						}
					);
			}
			return health;
		}
		
				
		/// <summary>
		/// Add Application Insights script to user context
		/// </summary>
		/// <returns>AI script</returns>
		/// <response code="200">Ok</response>
		[HttpGet("/api/health/application-insights")]
		[ResponseCache(Duration = 29030400)] // 4 weeks
		[Produces("application/javascript")]
		public IActionResult ApplicationInsights()
		{
			return Content(_applicationInsightsJsHelper.ScriptPlain, "application/javascript");
		}

		/// <summary>
		/// Check if min version is matching
		/// </summary>
		internal const string MinimumVersion = "0.3"; // only insert 0.4 or 0.5
		
		/// <summary>
		/// Name of the header for api version
		/// </summary>
		private const string ApiVersionHeaderName = "x-api-version";

		/// <summary>
		/// Check if Client/App version has a match with the API-version
		/// the parameter 'version' is checked first, and if missing the x-api-version header is used
		/// </summary>
		/// <returns>status</returns>
		/// <response code="200">Ok</response>
		/// <response code="202">Version mismatch</response>
		/// <response code="400">Missing x-api-version header OR bad formatted version in header</response>
		[HttpPost("/api/health/version")]
		public IActionResult Version(string version = null)
		{
			if ( string.IsNullOrEmpty(version) )
			{
				var headerVersion =
					Request.Headers.FirstOrDefault(p =>
						p.Key == ApiVersionHeaderName).Value;
				if (!string.IsNullOrEmpty(headerVersion))
				{
					version = headerVersion;
				}
			}

			if ( string.IsNullOrEmpty(version))
			{
				return BadRequest("Missing version data");
			}

			try
			{
				if ( SemVersion.Parse(version) >= SemVersion.Parse(MinimumVersion) )
				{
					return Ok(version);
				}
				return StatusCode(StatusCodes.Status202Accepted,
					$"please upgrade to {MinimumVersion} or newer");
			}
			catch ( ArgumentException )
			{
				return StatusCode(StatusCodes.Status400BadRequest,
					$"Parsing failed {version}");
			}
		}
	}
}
