using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

/// <summary>
///     Live crash report checks, all disabled by default so a normal test run never
///     talks to the network. Run them with an environment variable:
///     <list type="bullet">
///         <item>
///             <c>STARSKY_CRASH_REPORT_LIVE_TEST=1</c> starts a local sink and lets a child
///             process crash on an unhandled exception; nothing leaves the machine
///         </item>
///         <item>
///             <c>STARSKY_CRASH_REPORT_DSN</c> sends one marked test crash to that DSN,
///             this reports to a real Sentry project
///         </item>
///     </list>
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class SetupSentryLiveTest
{
	internal const string OptInVariable = "STARSKY_CRASH_REPORT_LIVE_TEST";
	internal const string RealDsnVariable = "STARSKY_CRASH_REPORT_DSN";

	private const string ChildVariable = "STARSKY_CRASH_REPORT_CHILD";
	private const string ChildDsnVariable = "app__Sentry__Dsn";
	private const string CrashMessage = "starsky live crash report test";
	private const string SecretFolder = "/Users/dion/secret-folder";
	private const string SecretFileName = "starsky-live-test.jpg";

	/// <summary>
	///     A real process dies from an unhandled exception and the crash report is
	///     received over HTTP. The child is this test assembly, the sink replaces Sentry.
	/// </summary>
	[TestMethod]
	[Timeout(180_000, CooperativeCancellation = true)]
	public async Task Live_UnhandledCrash_InChildProcess_IsReportedToSink()
	{
		if ( Environment.GetEnvironmentVariable(OptInVariable) != "1" )
		{
			Assert.Inconclusive($"set {OptInVariable}=1 to run the live crash report test");
			return;
		}

		using var sink = new SentryHttpSink();
		var child = StartChildProcessThatCrashes(sink.Dsn);
		try
		{
			var envelope = await sink.WaitForEnvelopeAsync(TimeSpan.FromSeconds(90));
			Assert.IsNotNull(envelope,
				$"no crash report arrived on {sink.Dsn}{Environment.NewLine}{await DescribeChildAsync(child)}");

			Assert.AreEqual("/api/1/envelope/", sink.RequestPath);
			Assert.Contains("InvalidOperationException", envelope);
			Assert.Contains(CrashMessage, envelope);
			Assert.DoesNotContain(SecretFileName, envelope,
				"the file path in the exception message was not scrubbed");

			// the envelope is json, that escapes the angle brackets of the placeholder
			var unescaped = envelope
				.Replace("\\u003C", "<", StringComparison.Ordinal)
				.Replace("\\u003E", ">", StringComparison.Ordinal);
			Assert.Contains(SetupSentry.PathPlaceholder, unescaped);

			Assert.IsTrue(child.WaitForExit(60_000),
				$"the child process did not crash{Environment.NewLine}{await DescribeChildAsync(child)}");
			Assert.AreNotEqual(0, child.ExitCode, "the child process should have crashed");
		}
		finally
		{
			if ( !child.HasExited )
			{
				child.Kill(true);
			}

			child.Dispose();
		}
	}

	/// <summary>
	///     Runs the crash in a separate process, a parent process can not survive
	///     an unhandled exception. Only starts when the parent sets <c>STARSKY_CRASH_REPORT_CHILD</c>.
	/// </summary>
	[TestMethod]
	[Timeout(120_000, CooperativeCancellation = true)]
	public async Task LiveChild_UnhandledCrash_IsReportedToSink()
	{
		if ( Environment.GetEnvironmentVariable(ChildVariable) != "1" )
		{
			Assert.Inconclusive(
				$"only used as a child process of {nameof(Live_UnhandledCrash_InChildProcess_IsReportedToSink)}");
			return;
		}

		// The same way a released build gets its Dsn, see starsky/telemetry.md
		var configuration = await SetupAppSettings.AppSettingsToBuilder();
		var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(
			new ServiceCollection(), configuration);
		Assert.IsFalse(string.IsNullOrWhiteSpace(appSettings.Sentry?.Dsn), ChildDsnVariable + " is not set");

		using var crashReports = SetupSentry.Init(appSettings);
		Assert.IsNotNull(crashReports, "crash reporting did not start");

		var crashingThread = new Thread(() => throw new InvalidOperationException(
			$"{CrashMessage} {Path.Combine(SecretFolder, SecretFileName)}"))
		{
			// keep the process alive until the runtime kills it
			IsBackground = false
		};
		crashingThread.Start();
		crashingThread.Join();
	}

	/// <summary>
	///     Sends one marked crash report to the DSN in <c>STARSKY_CRASH_REPORT_DSN</c>.
	///     The test can only check that the report is accepted and sent, delivery itself
	///     is visible in the Sentry project with the tag <c>starsky_live_test=true</c>.
	/// </summary>
	[TestMethod]
	[Timeout(120_000, CooperativeCancellation = true)]
	public async Task Live_TestCrash_IsSentToConfiguredDsn()
	{
		var dsn = Environment.GetEnvironmentVariable(RealDsnVariable);
		if ( string.IsNullOrWhiteSpace(dsn) )
		{
			Assert.Inconclusive($"set {RealDsnVariable} to send one test crash report to your own project");
			return;
		}

		var appSettings = new AppSettings { Sentry = new SentrySettings { Dsn = dsn } };
		using ( var crashReports = SetupSentry.Init(appSettings) )
		{
			Assert.IsNotNull(crashReports, "crash reporting did not start");

			SentrySdk.ConfigureScope(scope =>
			{
				scope.SetTag("starsky_live_test", "true");
				scope.SetExtra("note", CrashMessage);
			});

			var eventId = SentrySdk.CaptureException(new InvalidOperationException(CrashMessage));
			Assert.AreNotEqual(SentryId.Empty, eventId, "the crash report was not accepted");

			await SentrySdk.FlushAsync(TimeSpan.FromSeconds(30));
		}
	}

	private static Process StartChildProcessThatCrashes(string dsn)
	{
		var assemblyPath = Assembly.GetExecutingAssembly().Location;
		var directory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
		var host = Path.GetFileNameWithoutExtension(assemblyPath) +
		           ( OperatingSystem.IsWindows() ? ".exe" : string.Empty );
		var appHost = Path.Combine(directory, host);

		var startInfo = new ProcessStartInfo
		{
			WorkingDirectory = directory,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		if ( File.Exists(appHost) )
		{
			startInfo.FileName = appHost;
		}
		else
		{
			startInfo.FileName = "dotnet";
			startInfo.ArgumentList.Add(assemblyPath);
		}

		startInfo.ArgumentList.Add("--filter");
		startInfo.ArgumentList.Add($"FullyQualifiedName~{nameof(LiveChild_UnhandledCrash_IsReportedToSink)}");
		startInfo.Environment[ChildVariable] = "1";
		startInfo.Environment[ChildDsnVariable] = dsn;

		return Process.Start(startInfo) ??
		       throw new InvalidOperationException("could not start the child process");
	}

	private static async Task<string> DescribeChildAsync(Process child)
	{
		var output = await child.StandardOutput.ReadToEndAsync();
		var error = await child.StandardError.ReadToEndAsync();
		return $"child exit code: {( child.HasExited ? child.ExitCode.ToString() : "still running" )}" +
		       $"{Environment.NewLine}stdout: {output}{Environment.NewLine}stderr: {error}";
	}

	/// <summary>
	///     Minimal Sentry ingest endpoint, appends the crash report to the DSN path
	/// </summary>
	private sealed class SentryHttpSink : IDisposable
	{
		private readonly TaskCompletionSource<string> _envelope = new();
		private readonly HttpListener _listener = new();

		public SentryHttpSink()
		{
			var port = FreePort();
			Dsn = $"http://publickey@127.0.0.1:{port}/1";
			_listener.Prefixes.Add($"http://127.0.0.1:{port}/");
			_listener.Start();
			_ = Task.Run(AcceptOneRequest);
		}

		public string Dsn { get; }

		public string? RequestPath { get; private set; }

		public void Dispose()
		{
			_listener.Close();
		}

		public async Task<string?> WaitForEnvelopeAsync(TimeSpan timeout)
		{
			var completed = await Task.WhenAny(_envelope.Task, Task.Delay(timeout));
			return completed == _envelope.Task ? await _envelope.Task : null;
		}

		private async Task AcceptOneRequest()
		{
			try
			{
				var context = await _listener.GetContextAsync();
				RequestPath = context.Request.Url?.AbsolutePath;

				await using var input = context.Request.InputStream;
				var contentEncoding = context.Request.Headers["Content-Encoding"];
				Stream body = contentEncoding?.Contains("gzip",
					StringComparison.OrdinalIgnoreCase) == true
					? new GZipStream(input, CompressionMode.Decompress)
					: input;

				using var reader = new StreamReader(body, Encoding.UTF8);
				var envelope = await reader.ReadToEndAsync();

				context.Response.StatusCode = 200;
				context.Response.ContentType = "application/json";
				context.Response.Close();

				_envelope.TrySetResult(envelope);
			}
			catch ( Exception exception )
			{
				_envelope.TrySetException(exception);
			}
		}

		private static int FreePort()
		{
			var probe = new TcpListener(IPAddress.Loopback, 0);
			probe.Start();
			var port = ( (IPEndPoint)probe.LocalEndpoint ).Port;
			probe.Stop();
			return port;
		}
	}
}
