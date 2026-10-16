using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Sentry;
using Sentry.Extensibility;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.webtelemetry.Helpers;

/// <summary>
///     Crash reporting: unhandled exceptions with scrubbed breadcrumbs, without personal data.
///     Does nothing when there is no DSN (Debug, forks, local builds) or when disabled.
/// </summary>
public static partial class SetupSentry
{
	internal const string AssemblyMetadataKey = "SentryDsn";

	/// <summary>
	///     Paths in messages are replaced by this prefix, a short hash and the file extension
	/// </summary>
	internal const string PathPlaceholderPrefix = "<path:";

	/// <summary>
	///     Breadcrumbs show what the application was doing before a crash
	/// </summary>
	internal const int MaxBreadcrumbsCount = 30;

	/// <summary>
	///     Top level folders with source of this repository
	/// </summary>
	private static readonly string[] SourceRoots =
		["starsky", "starsky-tools", "mac", "windows", "documentation"];

	/// <summary>
	///     Start crash reporting, dispose (or call Flush) before a short-lived process exits
	/// </summary>
	/// <returns>null when disabled</returns>
	public static IDisposable? Init(AppSettings appSettings,
		ITransport? transport = null, IWebLogger? webLogger = null)
	{
		var options = CreateOptions(appSettings,
			GetAssemblyDsn(typeof(SetupSentry).Assembly), transport, webLogger);
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
		string? assemblyDsn, ITransport? transport = null, IWebLogger? webLogger = null)
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
			MaxBreadcrumbs = MaxBreadcrumbsCount,
			TracesSampleRate = 0,
			AutoSessionTracking = false,
			AttachStacktrace = false,
			SendClientReports = false,
			IsGlobalModeEnabled = true
		};
		options.SetBeforeSend((sentryEvent, _) => ScrubAndLog(sentryEvent, webLogger));
		options.SetBeforeBreadcrumb((breadcrumb, _) => ScrubBreadcrumb(breadcrumb));
		options.DefaultTags["app_type"] = appSettings.ApplicationType.ToString();
		options.DefaultTags["commit"] = appSettings.AppVersionCommitHash;

		if ( transport != null )
		{
			options.Transport = transport;
		}

		return options;
	}

	/// <summary>
	///     Scrub and write the crash report id to the local log. That id is the only thing a
	///     user has to share, and the log line itself contains no personal data.
	/// </summary>
	internal static SentryEvent ScrubAndLog(SentryEvent sentryEvent, IWebLogger? webLogger)
	{
		var scrubbed = Scrub(sentryEvent);
		if ( webLogger == null )
		{
			return scrubbed;
		}

		var exception = scrubbed.SentryExceptions?.FirstOrDefault();
		webLogger.LogInformation(
			$"crash report {scrubbed.EventId} {exception?.Type}: {exception?.Value}");
		return scrubbed;
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
				// the file name points at the build machine, the part inside the repository
				// points at the code
				frame.FileName = NormalizeSourcePath(frame.FileName);
				frame.AbsolutePath = null;
				frame.ContextLine = null;
				frame.PreContext.Clear();
				frame.PostContext.Clear();
				frame.Vars.Clear();
			}
		}

		// debug images point to the assembly on this machine, including the user name
		foreach ( var image in sentryEvent.DebugImages ?? [] )
		{
			image.DebugFile = null;
			image.CodeFile = null;
		}

		return sentryEvent;
	}

	/// <summary>
	///     Breadcrumbs show what the application was doing. Requests and database queries are
	///     dropped, they carry the urls, search terms and file paths of the user.
	/// </summary>
	internal static Breadcrumb? ScrubBreadcrumb(Breadcrumb? breadcrumb)
	{
		if ( breadcrumb == null )
		{
			return null;
		}

		if ( breadcrumb.Type == BreadcrumbTypes.Http || breadcrumb.Type == BreadcrumbTypes.Query )
		{
			return null;
		}

		// ASP.NET Core logs the url of every request, including its query string
		if ( breadcrumb.Category?.StartsWith("Microsoft.AspNetCore",
			    StringComparison.Ordinal) == true )
		{
			return null;
		}

		// breadcrumbs are read only, so a scrubbed copy is returned. Data holds structured
		// values that can point at the files of the user, it is dropped instead of scrubbed
		return new Breadcrumb(
			RemovePaths(breadcrumb.Message) ?? string.Empty,
			breadcrumb.Type ?? BreadcrumbTypes.Default,
			category: breadcrumb.Category,
			level: breadcrumb.Level);
	}

	/// <summary>
	///     A stack trace file name is a path on the build machine (CI or a developer), never a
	///     file of the user. Keep the part that points into the repository, drop the rest.
	/// </summary>
	internal static string? NormalizeSourcePath(string? fileName)
	{
		if ( string.IsNullOrWhiteSpace(fileName) )
		{
			return null;
		}

		var segments = fileName.Trim().Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
		if ( segments.Length == 0 )
		{
			return null;
		}

		// the last match, a build path repeats folder names (C:\a\starsky\starsky\...)
		var start = Array.FindLastIndex(segments, segment => SourceRoots.Contains(segment));
		if ( start >= 0 )
		{
			return string.Join('/', segments, start, segments.Length - start);
		}

		// without a directory the name says nothing about the machine
		return segments.Length == 1 ? segments[0] : null;
	}

	internal static string? RemovePaths(string? text)
	{
		return text == null
			? null
			: PathRegex().Replace(text, PathReplacement);
	}

	/// <summary>
	///     The same path always gives the same hash, so two crashes on the same file stay one
	///     report and another file makes a new one, without revealing which file it is
	/// </summary>
	private static string PathReplacement(Match match)
	{
		var path = match.Value;
		var extension = Path.GetExtension(path);
		if ( !ExtensionRegex().IsMatch(extension) )
		{
			extension = string.Empty;
		}

		return $"{PathPlaceholderPrefix}{ShortHash(path)}{extension}>";
	}

	internal static string ShortHash(string value)
	{
		return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..8];
	}

	// Windows (C:\Users\x\img.jpg), UNC and Unix absolute paths (/Users/x/img.jpg)
	[GeneratedRegex(@"(?:[A-Za-z]:\\|\\\\|/)[^\s'""<>|:*?]*[\\/][^\s'""<>|:*?]*")]
	private static partial Regex PathRegex();

	// ".jpg" and not the rest of a user invented name (".my holiday photo")
	[GeneratedRegex(@"^\.[a-zA-Z0-9]{1,6}$")]
	private static partial Regex ExtensionRegex();
}
