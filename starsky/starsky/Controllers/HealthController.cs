using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starskycore.Helpers;
using starskycore.ViewModels;

namespace starsky.Controllers
{
	public class HealthController: Controller
	{
		private readonly HealthCheckService _service;
		private readonly ApplicationInsightsJsHelper _applicationInsightsJsHelper;

		public HealthController(HealthCheckService service, ApplicationInsightsJsHelper applicationInsightsJsHelper = null)
		{
			_service = service;
			_applicationInsightsJsHelper = applicationInsightsJsHelper;
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
			if ( result.Status != HealthStatus.Healthy ) Response.StatusCode = 503;
			return Content(result.Status.ToString());
		}

		/// <summary>
		/// Check if the service has any known errors
		/// For Authorized Users only
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Ok</response>
		/// <response code="503">503 Service Unavailable</response>
		[HttpGet("/api/health/details")]
		[Authorize]
		[Produces("application/json")]
		[ProducesResponseType(typeof(HealthView),200)]
		[ProducesResponseType(typeof(HealthView),503)]
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
	}
}
