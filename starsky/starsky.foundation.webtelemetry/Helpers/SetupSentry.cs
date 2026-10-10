using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Sentry;
using Sentry.Extensibility;
using starsky.foundation.platform.Models;

namespace starsky.foundation.webtelemetry.Helpers;

/// <summary>
///     Crash reporting: only unhandled exceptions are sent, without personal data.
///     Does nothing when there is no DSN (Debug, forks, local builds) or when disabled.
/// </summary>
public static partial class SetupSentry
{
	internal const string AssemblyMetadataKey = "SentryDsn";
	internal const string PathPlaceholder = "<path>";

	/// <summary>
	///     Start crash reporting, dispose (or call Flush) before a short-lived process exits
	/// </summary>
	/// <returns>null when disabled</returns>
	public static IDisposable? Init(AppSettings appSettings,
		ITransport? transport = null)
	{
		var options = CreateOptions(appSettings,
			GetAssemblyDsn(typeof(SetupSentry).Assembly), transport);
		return options == null ? null : SentrySdk.Init(options);
	}

	public static void Flush()
	{
		if ( SentrySdk.IsEnabled )
		{
			SentrySdk.FlushAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
		}
	}

	internal static string? GetAssemblyDsn(Assembly assembly)
	{
		return assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(p => p.Key == AssemblyMetadataKey)?.Value?.Trim();
	}

	internal static SentryOptions? CreateOptions(AppSettings appSettings,
		string? assemblyDsn, ITransport? transport = null)
	{
		if ( appSettings.EnableCrashReports == false )
		{
			return null;
		}

		var dsn = string.IsNullOrWhiteSpace(appSettings.Sentry?.Dsn)
			? assemblyDsn
			: appSettings.Sentry.Dsn;

		if ( string.IsNullOrWhiteSpace(dsn) )
		{
			return null;
		}

		var options = new SentryOptions
		{
			Dsn = dsn,
			Release = appSettings.AppVersion,
			Environment = ( appSettings.Sentry ?? new SentrySettings() ).GetEnvironmentName(),
			// Privacy: crashes only
			SendDefaultPii = false,
			MaxBreadcrumbs = 0,
			TracesSampleRate = 0,
			AutoSessionTracking = false,
			AttachStacktrace = false,
			SendClientReports = false,
			IsGlobalModeEnabled = true
		};
		options.SetBeforeSend((sentryEvent, _) => Scrub(sentryEvent));
		options.SetBeforeBreadcrumb((_, _) => null);
		options.DefaultTags["app_type"] = appSettings.ApplicationType.ToString();
		options.DefaultTags["commit"] = appSettings.AppVersionCommitHash;

		if ( transport != null )
		{
			options.Transport = transport;
		}

		return options;
	}

	/// <summary>
	///     Remove everything that could identify a person, machine or file
	/// </summary>
	internal static SentryEvent Scrub(SentryEvent sentryEvent)
	{
		sentryEvent.User = new SentryUser();
		sentryEvent.ServerName = null;
		sentryEvent.Request = new SentryRequest();
		sentryEvent.Contexts.Device.Name = null;
		sentryEvent.Contexts.Device.Timezone = null;

		sentryEvent.Message = sentryEvent.Message == null
			? null
			: new SentryMessage { Formatted = RemovePaths(sentryEvent.Message.Formatted) };

		foreach ( var exception in sentryEvent.SentryExceptions ?? [] )
		{
			exception.Value = RemovePaths(exception.Value);
			exception.Mechanism?.Data.Clear();
			foreach ( var frame in exception.Stacktrace?.Frames ?? [] )
			{
				frame.AbsolutePath = null;
				frame.FileName = null;
				frame.ContextLine = null;
				frame.PreContext.Clear();
				frame.PostContext.Clear();
				frame.Vars.Clear();
			}
		}

		return sentryEvent;
	}

	internal static string? RemovePaths(string? text)
	{
		return text == null
			? null
			: PathRegex().Replace(text, PathPlaceholder);
	}

	// Windows (C:\Users\x\img.jpg), UNC and Unix absolute paths (/Users/x/img.jpg)
	[GeneratedRegex(@"(?:[A-Za-z]:\\|\\\\|/)[^\s'""<>|:*?]*[\\/][^\s'""<>|:*?]*")]
	private static partial Regex PathRegex();
}
