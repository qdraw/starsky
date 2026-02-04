using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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

	[TestMethod]
	public void GetTelemetryAttributes_ValidAppSettings_ReturnsExpectedAttributes()
	{
		// Arrange
		const string testEnvironment = "test-environment";
		var appSettings = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings { EnvironmentName = testEnvironment }
		};

		// Act
		var result = SetupLogging.GetTelemetryAttributes(appSettings);

		// Assert
		Assert.HasCount(5, result);
		Assert.IsTrue(result.Any(kvp =>
			kvp.Key == SetupLogging.HostNameKey && kvp.Value.Equals(Environment.MachineName)));
		Assert.IsTrue(result.Any(kvp =>
			kvp.Key == SetupLogging.DeploymentEnvironmentName
			&& kvp.Value.Equals(testEnvironment)));
		Assert.IsTrue(result.Any(kvp => kvp.Key == SetupLogging.AppVersionName
		                                && kvp.Value.Equals(appSettings.AppVersion)));
		Assert.IsTrue(result.Any(kvp =>
			kvp.Key == SetupLogging.AppVersionBuildDateTimeName && kvp.Value.Equals(
				appSettings.AppVersionBuildDateTime.ToString(
					new CultureInfo("nl-NL")))));
		Assert.IsTrue(result.Any(kvp =>
			kvp.Key == SetupLogging.FrameworkDescriptionName &&
			kvp.Value.Equals(RuntimeInformation.FrameworkDescription)));
	}
}
