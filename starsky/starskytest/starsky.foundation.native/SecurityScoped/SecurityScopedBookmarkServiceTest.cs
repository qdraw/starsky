using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.SecurityScoped;
using starsky.foundation.platform.Architecture;

namespace starskytest.starsky.foundation.native.SecurityScoped;

[TestClass]
public class SecurityScopedBookmarkServiceTest
{
	private static SecurityScopedBookmarkService Create() =>
		new(new NullLogger<SecurityScopedBookmarkService>());

	// ── GetBookmarksDirectory ────────────────────────────────────────────────

	[TestMethod]
	public void GetBookmarksDirectory_NoEnvVar_ReturnsNull()
	{
		var previous = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		try
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", null);
			Assert.IsNull(SecurityScopedBookmarkService.GetBookmarksDirectory());
		}
		finally
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", previous);
		}
	}

	[TestMethod]
	public void GetBookmarksDirectory_WithEnvVar_ReturnsExpectedPath()
	{
		var previous = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		try
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", "group.test.app");
			var result = SecurityScopedBookmarkService.GetBookmarksDirectory();
			Assert.IsNotNull(result);
			Assert.Contains("Group Containers", result);
			Assert.Contains("group.test.app", result);
			Assert.IsTrue(result.EndsWith("bookmarks", StringComparison.Ordinal));
		}
		finally
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", previous);
		}
	}

	// ── StartAccessingInternal ───────────────────────────────────────────────

	[TestMethod]
	public async Task StartAccessingInternal_NonMacOs_CompletesImmediately()
	{
		var svc = Create();
		await svc.StartAccessingInternal(_ => false, "/any/path");
	}

	[TestMethod]
	public async Task StartAccessingInternal_NullDirectory_CompletesImmediately()
	{
		var svc = Create();
		await svc.StartAccessingInternal(_ => true, null);
	}

	[TestMethod]
	public async Task StartAccessingInternal_NonExistentDirectory_CompletesImmediately()
	{
		var svc = Create();
		await svc.StartAccessingInternal(_ => true, "/this/path/does/not/exist/starsky-test");
	}

	[TestMethod]
	public async Task StartAccessingInternal_EmptyDirectory_CompletesWithoutError()
	{
		var svc = Create();
		var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(dir);
		try
		{
			await svc.StartAccessingInternal(_ => true, dir);
		}
		finally
		{
			Directory.Delete(dir, recursive: true);
		}
	}

	[TestMethod]
	public async Task StartAccessingInternal_DirectoryWithNonBookmarkFiles_SkipsFiles()
	{
		var svc = Create();
		var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(dir);
		try
		{
			File.WriteAllText(Path.Combine(dir, "some-file.txt"), "hello");
			File.WriteAllText(Path.Combine(dir, "another.json"), "{}");
			// Only *.bookmark files are processed — these should be silently ignored
			await svc.StartAccessingInternal(_ => true, dir);
		}
		finally
		{
			Directory.Delete(dir, recursive: true);
		}
	}

	[TestMethod]
	public async Task StartAccessingInternal_InvalidBookmarkFile_LogsWarning_MacOsOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for macOS only");
			return;
		}

		var svc = Create();
		var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(dir);
		try
		{
			// Write garbage bytes — resolution will fail but must not throw
			File.WriteAllBytes(Path.Combine(dir, "bad.bookmark"), [0xDE, 0xAD, 0xBE, 0xEF]);
			await svc.StartAccessingInternal(_ => true, dir);
		}
		finally
		{
			Directory.Delete(dir, recursive: true);
		}
	}

	// ── StartAsync / StopAsync ───────────────────────────────────────────────

	[TestMethod]
	public async Task StartAsync_NoAppGroup_CompletesImmediately()
	{
		var previous = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		try
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", null);
			var svc = Create();
			await svc.StartAsync(CancellationToken.None);
		}
		finally
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", previous);
		}
	}

	[TestMethod]
	public async Task StopAsync_WithoutStart_DoesNotThrow()
	{
		var svc = Create();
		await svc.StopAsync(CancellationToken.None);
	}

	[TestMethod]
	public async Task StopAsync_AfterStart_DoesNotThrow()
	{
		var previous = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		try
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", null);
			var svc = Create();
			await svc.StartAsync(CancellationToken.None);
			await svc.StopAsync(CancellationToken.None);
		}
		finally
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", previous);
		}
	}

	// ── Dispose ──────────────────────────────────────────────────────────────

	[TestMethod]
	public void Dispose_WithoutStart_DoesNotThrow()
	{
		using var svc = Create();
	}

	[TestMethod]
	public void Dispose_CalledTwice_DoesNotThrow()
	{
		var svc = Create();
		svc.Dispose();
		svc.Dispose();
	}

	[TestMethod]
	public async Task Dispose_AfterStart_DoesNotThrow()
	{
		var previous = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		try
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", null);
			var svc = Create();
			await svc.StartAsync(CancellationToken.None);
			svc.Dispose();
		}
		finally
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", previous);
		}
	}

	// ── Full round-trip on macOS ──────────────────────────────────────────────

	[TestMethod]
	public async Task StartAsync_MacOsOnly_NonExistentBookmarksDir_CompletesWithoutError()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for macOS only");
			return;
		}

		var previous = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		try
		{
			// group container doesn't exist in CI — must not throw
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", "group.nl.qdraw.starsky.test");
			var svc = Create();
			await svc.StartAsync(CancellationToken.None);
		}
		finally
		{
			Environment.SetEnvironmentVariable("STARSKY_APP_GROUP", previous);
		}
	}
}
