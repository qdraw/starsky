using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

/// <summary>
///     Log lines stay on the machine, they only leave as part of a crash report.
///     See <see cref="SetupSentryLiveTest" /> for the crash itself.
/// </summary>
[TestClass]
[DoNotParallelize]
public class SentryBreadcrumbLoggerProviderTest
{
	private const string Dsn = "https://publickey@example.test/1";

	[TestMethod]
	public void Log_Information_IsAddedAsScrubbedBreadcrumb()
	{
		var appSettings = new AppSettings { Sentry = new SentrySettings { Dsn = Dsn } };
		using ( var crashReports = SetupSentry.Init(appSettings, new FakeSentryTransport()) )
		{
			Assert.IsNotNull(crashReports, "crash reporting did not start");

			var logger = new SentryBreadcrumbLoggerProvider()
				.CreateLogger("starsky.foundation.import");

			// debug lines would push the useful ones out of the buffer
			Assert.IsFalse(logger.IsEnabled(LogLevel.Debug));
			Assert.IsTrue(logger.IsEnabled(LogLevel.Information));

			logger.LogInformation("importing /Users/dion/photos/a.jpg");

			Breadcrumb? breadcrumb = null;
			SentrySdk.ConfigureScope(scope => breadcrumb = scope.Breadcrumbs.FirstOrDefault());

			Assert.IsNotNull(breadcrumb, "the log line did not become a breadcrumb");
			Assert.AreEqual("starsky.foundation.import", breadcrumb.Category);

			var message = breadcrumb.Message ?? string.Empty;
			Assert.DoesNotContain("/Users/dion", message);
			StringAssert.Contains(message, SetupSentry.PathPlaceholderPrefix);
		}
	}
}
