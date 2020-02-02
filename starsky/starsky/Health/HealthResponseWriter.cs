using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace starsky.Health
{
	/// <summary>
	/// Using a different output for Health API
	/// @see: https://docs.microsoft.com/nl-nl/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1#customize-output
	/// </summary>
	public class HealthResponseWriter
	{
		public static Task WriteResponse(HttpContext context, HealthReport result)
		{
			context.Response.ContentType = "application/json; charset=utf-8";
			
			

			var options = new JsonWriterOptions
			{
				Indented = true
			};

			using var stream = new MemoryStream();
			using (var writer = new Utf8JsonWriter(stream, options))
			{
				writer.WriteStartObject();
				writer.WriteString("status", result.Status.ToString());
				writer.WriteString("totalDuration", result.TotalDuration.ToString());
				writer.WriteStartObject("results");
				foreach (var entry in result.Entries)
				{
					writer.WriteStartObject(entry.Key);
					writer.WriteString("key", entry.Key);
					writer.WriteString("status", entry.Value.Status.ToString());
					writer.WriteEndObject();
				}
				writer.WriteEndObject();
				writer.WriteEndObject();
			}

			var json = Encoding.UTF8.GetString(stream.ToArray());

			return context.Response.WriteAsync(json);
		}

	}
	
	
}
