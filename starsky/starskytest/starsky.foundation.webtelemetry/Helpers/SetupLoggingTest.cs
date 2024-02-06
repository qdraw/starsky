using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTelemetry.Logs;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

[TestClass]
public class SetupLoggingTest
{
	[TestMethod]
	public void OpenTelemetry()
	{
		IServiceCollection services = new ServiceCollection();
		services.AddTelemetryLogging(new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				LogsEndpoint = "https://test.me/v1/logs"
			}
		});

		var build = services.BuildServiceProvider();

		var type = build.GetRequiredService<ILoggerProvider>();
		Assert.AreEqual(typeof(OpenTelemetryLoggerProvider), type.GetType());
	}
}
