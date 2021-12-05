using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using starsky.foundation.platform.Models;

namespace starsky.foundation.webtelemetry.Helpers
{
	public static class SetupLogging
	{
		public static void AddLogging( IServiceCollection services, AppSettings appSettings)
		{
			services.AddLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddConsole();
	            
				// Skip when is Development
				if (appSettings.ApplicationInsightsLog != true || 
				    string.IsNullOrWhiteSpace(appSettings.ApplicationInsightsInstrumentationKey)) return;
	            
				logging.AddApplicationInsights(appSettings.ApplicationInsightsInstrumentationKey);
			});
		}
	}
}
