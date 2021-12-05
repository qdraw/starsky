using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.webtelemetry.Helpers
{
	public class FlushOnApplicationStopping
	{
		private readonly IApplicationBuilder _app;

		public FlushOnApplicationStopping(IApplicationBuilder app)
		{
			_app = app;
		}

		public void Flush()
		{
			var client = _app.ApplicationServices.GetService<TelemetryClient>();
			client.FlushAsync(CancellationToken.None).GetAwaiter();
		}
	}
}
