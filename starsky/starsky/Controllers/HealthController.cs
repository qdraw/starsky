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
			_telemetryService.TrackException(
				new TelemetryServiceException(JsonSerializer.Serialize(
					result.Entries.Where(
						p => p.Value.Status != HealthStatus.Healthy
					)
				))
			);
			return Content(result.Status.ToString());
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

			var health = new HealthView
			{
				IsHealthy = result.Status == HealthStatus.Healthy,
				TotalDuration = result.TotalDuration
			};
			foreach ( var (key, value) in result.Entries )
			{
				health.Entries.Add(new HealthEntry{Duration = value.Duration, Name = key, IsHealthy = value.Status == HealthStatus.Healthy});
			}

			if ( !health.IsHealthy ) Response.StatusCode = 503;
			
			return Json(health);
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
		/// uses X-API-Version header
		/// </summary>
		/// <returns>AI script</returns>
		/// <response code="200">Ok</response>
		/// <response code="405">Version mismatch</response>
		/// <response code="400">Missing X-API-Version header or bad formated version in header</response>
		[HttpPost("/api/health/version")]
		public IActionResult Version()
		{
			if ( Request.Headers.All(p => p.Key != "X-API-Version") )
			{
				return BadRequest("missing version data");
			}
			return Ok();
		}
	}
}
