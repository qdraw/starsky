using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.webtelemetry.Helpers
{
	public class FlushApplicationInsights
	{
		private readonly ServiceProvider _serviceProvider;
		private readonly IApplicationBuilder _app;

		public FlushApplicationInsights(ServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public FlushApplicationInsights(IApplicationBuilder app)
		{
			_app = app;
		}

		private TelemetryClient GetTelemetryClient()
		{
			return _serviceProvider != null ? _serviceProvider.GetService<TelemetryClient>() : _app.ApplicationServices.GetService<TelemetryClient>();
		}

		public async Task FlushAsync()
		{
			var client = GetTelemetryClient();
			await client.FlushAsync(CancellationToken.None);
			await Task.Delay(10);
		}
		
		public void Flush()
		{
			var client = GetTelemetryClient();
			client.FlushAsync(CancellationToken.None).ConfigureAwait(false);
			Thread.Sleep(10);
		}
	}
}
