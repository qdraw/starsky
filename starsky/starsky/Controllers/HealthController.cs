using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starskycore.ViewModels;

namespace starsky.Controllers
{
	public class HealthController: Controller
	{
		private readonly HealthCheckService _service;

		public HealthController(HealthCheckService service)
		{
			_service = service;
		}
		
		/// <summary>
		/// Check if the service has any known errors
		/// Use `/health for anonymous calls 
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Ok</response>
		/// <response code="503">503 Service Unavailable</response>
		[HttpGet("/api/health")]
		[Authorize]
		[Produces("application/json")]
		[ProducesResponseType(typeof(HealthView),200)]
		[ProducesResponseType(typeof(HealthView),503)]
		public IActionResult Index()
		{
			var result =  _service.CheckHealthAsync().Result;

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
	}
}
