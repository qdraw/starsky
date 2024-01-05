using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Trace.V1;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Interfaces;

namespace starsky.Controllers;

public class OpenTelemetryController: Controller
{
	private readonly AppSettings _appSettings;
	private readonly ITracesService _tracesService;

	public OpenTelemetryController(AppSettings appSettings, ITracesService tracesService)
	{
		_appSettings = appSettings;
		_tracesService = tracesService;
	}
	
	/// <summary>
	/// /api/open-telemetry/trace
	/// </summary>
	/// <returns>trace</returns>
	/// <response code="200">Ok</response>
	[HttpPost("/api/open-telemetry/trace")]
	[AllowAnonymous]
	[Produces("application/json")]
	public async Task<IActionResult> Trace()
	{
		using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
		{
			var jsonInput = await stream.ReadToEndAsync();
			// Needs to be Newtonsoft.Json because of the Protobuf
			var tracesData = JsonConvert.DeserializeObject<TracesData>(jsonInput, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			});
			
			if ( !tracesData.ResourceSpans.Any() ) return BadRequest(tracesData);
			
			// tracesData.ResourceSpans.FirstOrDefault()?.Resource.Attributes
			// 	.Remove(
			// 		tracesData.ResourceSpans.FirstOrDefault()?.Resource
			// 			.Attributes
			// 			.FirstOrDefault(p => p.Key == "service.name"));
			// 	
			// tracesData.ResourceSpans.FirstOrDefault()?.Resource.Attributes.Add(new KeyValue
			// {
			// 	Key = "service.name",
			// 	Value = new AnyValue { StringValue = _appSettings.OpenTelemetry.GetServiceName() + "-client-app" }
			// });
			
			await _tracesService.SendTrace(tracesData);

			return Ok(tracesData);
		}
		
		// Example: Convert Protobuf object to JSON string for demonstration

	}
	
}
