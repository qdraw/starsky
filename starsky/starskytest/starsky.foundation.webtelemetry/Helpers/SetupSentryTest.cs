using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry;
using Sentry.Protocol;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

[TestClass]
[DoNotParallelize]
public class SetupSentryTest
{
	private const string Dsn = "https://publickey@example.test/1";

	[TestMethod]
	public void CreateOptions_NoDsn_Disabled()
	{
		Assert.IsNull(SetupSentry.CreateOptions(new AppSettings(), null));
		Assert.IsNull(SetupSentry.CreateOptions(new AppSettings(), "  "));
	}

	[TestMethod]
	public void CreateOptions_EnableCrashReportsFalse_Disabled()
	{
		var appSettings = new AppSettings { EnableCrashReports = false };
		Assert.IsNull(SetupSentry.CreateOptions(appSettings, Dsn));
	}

	[TestMethod]
	public void CreateOptions_DefaultEnabled_WithAssemblyDsn()
	{
		var options = SetupSentry.CreateOptions(new AppSettings(), Dsn);

		Assert.IsNotNull(options);
		Assert.AreEqual(Dsn, options.Dsn);
	}

	[TestMethod]
	public void CreateOptions_AppSettingsDsnOverwritesAssemblyDsn()
	{
		var appSettings = new AppSettings
		{
			Sentry = new SentrySettings { Dsn = "https://other@example.test/2" }
		};

		var options = SetupSentry.CreateOptions(appSettings, Dsn);

		Assert.AreEqual("https://other@example.test/2", options?.Dsn);
	}

	[TestMethod]
	public void CreateOptions_PrivacyOptions()
	{
		var options = SetupSentry.CreateOptions(new AppSettings(), Dsn);

		Assert.IsNotNull(options);
		Assert.IsFalse(options.SendDefaultPii);
		Assert.AreEqual(0, options.MaxBreadcrumbs);
		Assert.AreEqual(0d, options.TracesSampleRate);
		Assert.IsFalse(options.AutoSessionTracking);
	}

	[TestMethod]
	public void Scrub_RemovesPersonalData()
	{
		var sentryEvent = new SentryEvent
		{
			User = new SentryUser { Username = "dion", IpAddress = "127.0.0.1" },
			ServerName = "my-macbook",
			Request = new SentryRequest { Url = "http://localhost/api/?f=/Users/dion/a.jpg" },
			Message = new SentryMessage { Formatted = "failed /Users/dion/photos/a.jpg" },
			SentryExceptions =
			[
				new SentryException
				{
					Value = @"Could not find C:\Users\dion\a.jpg or /Users/dion/b.jpg",
					Stacktrace = new SentryStackTrace
					{
						Frames =
						[
							new SentryStackFrame
							{
								AbsolutePath = "/Users/dion/src/Program.cs",
								FileName = "Program.cs",
								Function = "Main"
							}
						]
					}
				}
			]
		};
		sentryEvent.Contexts.Device.Name = "my-macbook";

		var result = SetupSentry.Scrub(sentryEvent);

		Assert.IsNull(result.User.Username);
		Assert.IsNull(result.User.IpAddress);
		Assert.IsNull(result.ServerName);
		Assert.IsNull(result.Request.Url);
		Assert.IsNull(result.Contexts.Device.Name);
		Assert.AreEqual("failed <path>", result.Message?.Formatted);
		Assert.AreEqual("Could not find <path> or <path>", result.SentryExceptions?.First().Value);

		var frame = result.SentryExceptions?.First().Stacktrace?.Frames[0];
		Assert.IsNull(frame?.AbsolutePath);
		Assert.IsNull(frame?.FileName);
		// code location without a file name is kept
		Assert.AreEqual("Main", frame?.Function);
	}

	[TestMethod]
	[DataRow("no path here", "no path here")]
	[DataRow("see /Users/dion/a.jpg now", "see <path> now")]
	[DataRow(@"see C:\Users\dion\a.jpg now", "see <path> now")]
	[DataRow(@"see \\server\share\a.jpg now", "see <path> now")]
	[DataRow("and/or", "and/or")]
	public void RemovePaths(string input, string expected)
	{
		Assert.AreEqual(expected, SetupSentry.RemovePaths(input));
	}

	[TestMethod]
	public void RemovePaths_Null()
	{
		Assert.IsNull(SetupSentry.RemovePaths(null));
	}

	[TestMethod]
	public void GetAssemblyDsn_NotSet_ReturnsNull()
	{
		Assert.IsNull(SetupSentry.GetAssemblyDsn(Assembly.GetExecutingAssembly()));
	}

	[TestMethod]
	public void Init_Disabled_ReturnsNull()
	{
		using var result = SetupSentry.Init(new AppSettings { EnableCrashReports = false });
		Assert.IsNull(result);
	}

	[TestMethod]
	public void Init_Enabled_CapturesAndFlushes()
	{
		var transport = new FakeSentryTransport();
		var appSettings = new AppSettings
		{
			Sentry = new SentrySettings { Dsn = Dsn }
		};

		using ( var result = SetupSentry.Init(appSettings, transport) )
		{
			Assert.IsNotNull(result);
			SentrySdk.CaptureException(new InvalidOperationException("test"));
			SetupSentry.Flush();
		}

		Assert.HasCount(1, transport.Envelopes);
	}

	[TestMethod]
	public void Flush_NotInitialized_DoesNotThrow()
	{
		SetupSentry.Flush();
	}
}
