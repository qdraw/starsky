using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
		Assert.AreEqual(SetupSentry.MaxBreadcrumbsCount, options.MaxBreadcrumbs);
		Assert.AreEqual(0d, options.TracesSampleRate);
		Assert.IsFalse(options.AutoSessionTracking);
	}

	[TestMethod]
	public void Scrub_RemovesDebugImagePaths()
	{
		var sentryEvent = new SentryEvent
		{
			DebugImages =
			[
				new DebugImage
				{
					DebugFile = "/Users/dion/app/starsky.pdb",
					CodeFile = @"C:\Users\dion\app\starsky.dll",
					DebugId = "32c2e2ca-7dd2-4e4e-ad34-3358be7c7713-9d786101"
				}
			]
		};

		var result = SetupSentry.Scrub(sentryEvent);

		var image = result.DebugImages?.First();
		Assert.IsNull(image?.DebugFile);
		Assert.IsNull(image?.CodeFile);
		// without the paths the debug id is still there for symbolication
		Assert.AreEqual("32c2e2ca-7dd2-4e4e-ad34-3358be7c7713-9d786101", image?.DebugId);
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
								AbsolutePath = "/Users/dion/data/git/starsky/starsky/Program.cs",
								FileName = "/Users/dion/data/git/starsky/starsky/Program.cs",
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
		// the file type stays and the hash says which file it was without naming it
		StringAssert.Matches(result.Message?.Formatted,
			new Regex(@"^failed <path:[0-9a-f]{8}\.jpg>$"));
		StringAssert.Matches(result.SentryExceptions?.First().Value,
			new Regex(@"^Could not find <path:[0-9a-f]{8}\.jpg> or <path:[0-9a-f]{8}\.jpg>$"));

		var frame = result.SentryExceptions?.First().Stacktrace?.Frames[0];
		Assert.IsNull(frame?.AbsolutePath);
		// the folder of the build machine is gone, the file inside the repository stays
		Assert.AreEqual("starsky/Program.cs", frame?.FileName);
		Assert.AreEqual("Main", frame?.Function);
	}

	[TestMethod]
	[DataRow("no path here", "no path here")]
	[DataRow("and/or", "and/or")]
	public void RemovePaths_NoPath_Unchanged(string input, string expected)
	{
		Assert.AreEqual(expected, SetupSentry.RemovePaths(input));
	}

	[TestMethod]
	[DataRow("see /Users/dion/a.jpg now")]
	[DataRow(@"see C:\Users\dion\a.jpg now")]
	[DataRow(@"see \\server\share\a.jpg now")]
	public void RemovePaths_KeepsTheFileType(string input)
	{
		var result = SetupSentry.RemovePaths(input);

		StringAssert.Matches(result, new Regex(@"^see <path:[0-9a-f]{8}\.jpg> now$"));
	}

	[TestMethod]
	public void RemovePaths_NoExtension_NoExtensionInPlaceholder()
	{
		var result = SetupSentry.RemovePaths("/Users/dion/photos/import");

		StringAssert.Matches(result, new Regex(@"^<path:[0-9a-f]{8}>$"));
	}

	[TestMethod]
	public void RemovePaths_SameFile_SameHash()
	{
		var first = SetupSentry.RemovePaths("/Users/dion/photos/a.jpg");
		var sameFile = SetupSentry.RemovePaths("/Users/dion/photos/a.jpg");
		var otherFile = SetupSentry.RemovePaths("/Users/dion/photos/b.jpg");

		// the same hash keeps crashes on one file in one report, another file makes a new one
		Assert.AreEqual(first, sameFile);
		Assert.AreNotEqual(first, otherFile);
	}

	[TestMethod]
	public void RemovePaths_Null()
	{
		Assert.IsNull(SetupSentry.RemovePaths(null));
	}

	[TestMethod]
	[DataRow("/Users/dion/data/git/starsky/starsky/Controllers/FooController.cs",
		"starsky/Controllers/FooController.cs")]
	[DataRow(@"C:\Users\dion\data\git\starsky\starsky\Controllers\FooController.cs",
		"starsky/Controllers/FooController.cs")]
	[DataRow("/home/runner/work/starsky/starsky/starsky.foundation.platform/Models/AppSettings.cs",
		"starsky/starsky.foundation.platform/Models/AppSettings.cs")]
	[DataRow("/Users/mac/data/git/starsky/starsky/Program.cs", "starsky/Program.cs")]
	[DataRow("starsky/Program.cs", "starsky/Program.cs")]
	[DataRow("Program.cs", "Program.cs")]
	[DataRow("/Users/dion/.nuget/packages/sentry/1.0/SentrySdk.cs", (string?)null)]
	[DataRow("/Users/dion/Photos/holiday.jpg", (string?)null)]
	public void NormalizeSourcePath(string? input, string? expected)
	{
		Assert.AreEqual(expected, SetupSentry.NormalizeSourcePath(input));
	}

	[TestMethod]
	[DataRow(BreadcrumbTypes.Http)]
	[DataRow(BreadcrumbTypes.Query)]
	public void ScrubBreadcrumb_RequestOrQuery_Removed(string type)
	{
		var breadcrumb = new Breadcrumb("GET http://localhost/api/q?f=/Users/dion/a.jpg", type,
			new Dictionary<string, string> { { "url", "http://localhost/?f=/Users/dion/a.jpg" } },
			"System.Net.Http.HttpClient");

		Assert.IsNull(SetupSentry.ScrubBreadcrumb(breadcrumb));
	}

	[TestMethod]
	public void ScrubBreadcrumb_AspNetCoreRequest_Removed()
	{
		// request urls carry search terms and file paths of the user
		var breadcrumb = new Breadcrumb("Request starting GET /api/search?q=holiday",
			BreadcrumbTypes.Info, category: "Microsoft.AspNetCore.Hosting.Diagnostics");

		Assert.IsNull(SetupSentry.ScrubBreadcrumb(breadcrumb));
	}

	[TestMethod]
	public void ScrubBreadcrumb_LogLine_ScrubbedAndKept()
	{
		var breadcrumb = new Breadcrumb("importing /Users/dion/photos/a.jpg",
			BreadcrumbTypes.Info,
			new Dictionary<string, string> { { "file", "/Users/dion/photos/a.jpg" } },
			"starsky.foundation.import");

		var result = SetupSentry.ScrubBreadcrumb(breadcrumb);

		Assert.IsNotNull(result);
		Assert.AreEqual("starsky.foundation.import", result.Category);
		Assert.AreEqual(BreadcrumbTypes.Info, result.Type);
		StringAssert.Matches(result.Message,
			new Regex(@"^importing <path:[0-9a-f]{8}\.jpg>$"));
		// data holds structured values that can point at the files of the user
		Assert.IsTrue(result.Data == null || result.Data.Count == 0);
		// a timestamp of the line itself, the scrubbed copy should not fall back to a default
		Assert.IsGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1), result.Timestamp);
	}

	[TestMethod]
	public void ScrubBreadcrumb_Null()
	{
		Assert.IsNull(SetupSentry.ScrubBreadcrumb(null));
	}

	[TestMethod]
	public void ScrubAndLog_WritesTheEventIdToTheLog()
	{
		var webLogger = new FakeIWebLogger();
		var sentryEvent = new SentryEvent
		{
			SentryExceptions =
			[
				new SentryException
				{
					Type = "System.InvalidOperationException",
					Value = "failed /Users/dion/photos/a.jpg"
				}
			]
		};

		var result = SetupSentry.ScrubAndLog(sentryEvent, webLogger);

		Assert.HasCount(1, webLogger.TrackedInformation);
		var message = webLogger.TrackedInformation[0].Item2;
		StringAssert.Contains(message, result.EventId.ToString());
		StringAssert.Contains(message, "System.InvalidOperationException");
		// a user can paste this line in a bug report
		Assert.DoesNotContain("/Users/dion", message);
	}

	[TestMethod]
	public void ScrubAndLog_WithoutLogger_DoesNotThrow()
	{
		Assert.IsNotNull(SetupSentry.ScrubAndLog(new SentryEvent(), null));
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
		// Flush is called on shutdown paths, so it must be safe before Init ran
		Exception? caught = null;
		try
		{
			SetupSentry.Flush();
		}
		catch ( Exception ex )
		{
			caught = ex;
		}

		Assert.IsNull(caught);
	}
}
