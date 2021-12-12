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

		public FlushApplicationInsights(ServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task Flush()
		{
			var client = _serviceProvider.GetService<TelemetryClient>();
			await client.FlushAsync(CancellationToken.None);
			await Task.Delay(10);
		}
	}
}
