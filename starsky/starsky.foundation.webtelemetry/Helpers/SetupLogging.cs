using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;

namespace starsky.foundation.webtelemetry.Helpers
{
	public static class SetupLogging
	{
		[SuppressMessage("Usage", "S4792:Make sure that this logger's configuration is safe.")]
		public static void AddApplicationInsightsLogging(this IServiceCollection services, AppSettings appSettings)
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

			services.AddScoped<IWebLogger, WebLogger>();
		}
	}
}
