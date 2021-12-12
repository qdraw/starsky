using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.consoletelemetry.Helpers
{
	public class FlushOnApplicationStopping
	{
		private readonly ServiceProvider _serviceProvider;

		public FlushOnApplicationStopping(ServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void Flush()
		{
			var client = _serviceProvider.GetService<TelemetryClient>();
			client.FlushAsync(CancellationToken.None).GetAwaiter();
		}
	}
}
