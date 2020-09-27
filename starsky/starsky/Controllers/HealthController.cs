using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Exceptions;
using starsky.foundation.platform.Interfaces;
using starskycore.Helpers;
using starskycore.ViewModels;

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
			var result = await _service.CheckHealthAsync();
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
		/// Check if Client/App version has a match with the API-version
		/// uses x-api-version header
		/// </summary>
		/// <returns>AI script</returns>
		/// <response code="200">Ok</response>
		/// <response code="405">Version mismatch</response>
		/// <response code="400">Missing x-api-version header or bad formated version in header</response>
		[HttpPost("/api/health/version")]
		public IActionResult Version()
		{
			var headerName = "x-api-version";
			
			if ( Request.Headers.All(p => p.Key != headerName) 
			     || string.IsNullOrWhiteSpace(Request.Headers[headerName])  )
			{
				HeaderFailLogging(headerName);
				return BadRequest("Missing version data");
			}
			return Ok(Request.Headers[headerName]);
		}

		private void HeaderFailLogging(string headerName)
		{
			Console.WriteLine($"/api/health/version {headerName} Header Check Fail");
			if ( string.IsNullOrWhiteSpace(Request.Headers[headerName]) )
				Console.WriteLine($"IsNullOrWhiteSpace: {Request.Headers[headerName]}");
		}
	}
}
