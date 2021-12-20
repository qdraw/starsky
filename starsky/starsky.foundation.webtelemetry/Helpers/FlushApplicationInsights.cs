using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.webtelemetry.Helpers
{
	public class FlushApplicationInsights
	{
		private readonly ServiceProvider _serviceProvider;
		private readonly IApplicationBuilder _app;
		private readonly IWebLogger _logger;
		private readonly AppSettings _appSettings;

		public FlushApplicationInsights(ServiceProvider serviceProvider, AppSettings appSettings = null, IWebLogger logger = null)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
			_appSettings = appSettings;
		}

		public FlushApplicationInsights(IApplicationBuilder app)
		{
			_app = app;
		}

		internal TelemetryClient GetTelemetryClient()
		{
			var client = _serviceProvider != null ? _serviceProvider.GetService<TelemetryClient>() : _app.ApplicationServices.GetService<TelemetryClient>();
			if ( client == null && _appSettings != null && !string.IsNullOrEmpty(_appSettings.ApplicationInsightsInstrumentationKey) )
			{
				_logger.LogInformation("TelemetryClient is null on exit");
			}
			return client;
		}

		public async Task FlushAsync()
		{
			var client = GetTelemetryClient();
			if ( client == null ) return;
			await client.FlushAsync(CancellationToken.None);
			await Task.Delay(10);
		}
		
		public void Flush()
		{
			var client = GetTelemetryClient();
			if ( client == null ) return;
			client.FlushAsync(CancellationToken.None).ConfigureAwait(false);
			Thread.Sleep(10);
		}
	}
}
