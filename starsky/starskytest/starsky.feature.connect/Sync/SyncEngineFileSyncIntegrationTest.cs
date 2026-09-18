using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.connect.Sync;
using starsky.foundation.connect.Crypto;
using starsky.foundation.connect.Protocol;
using starsky.foundation.connect.Sync;
using starsky.foundation.connect.Transport;
using starsky.foundation.database.Data;

namespace starskytest.starsky.feature.connect.Sync;

/// <summary>
/// Integration tests that boot a real Syncthing process and exercise bidirectional file sync
/// through the full BEP stack: Scanner → SyncEngine → ConnectionManager → Syncthing.
///
/// Scenario A: Syncthing has a file → SyncEngine downloads and assembles it locally.
/// Scenario B: Starsky has a file → SyncEngine sends it; Syncthing writes it to its folder.
///
/// Requires `syncthing` on PATH. Marks as Inconclusive when not found.
/// Folder and device are configured via the Syncthing REST API after startup (version-independent).
/// On macOS (SecureTransport TLS 1.2 fallback), falls back to Homebrew openssl s_client.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public sealed class SyncEngineFileSyncIntegrationTest
{
	private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan SyncTimeout = TimeSpan.FromSeconds(60);
	private const string FolderId = "starskytest";

	private string? _tempDir;
	private Process? _syncthingProcess;
	private SqliteConnection? _sqliteConn;
	private ApplicationDbContext? _db;

	[TestCleanup]
	public void Cleanup()
	{
		try { _syncthingProcess?.Kill(entireProcessTree: true); } catch { }
		_syncthingProcess?.Dispose();
		_db?.Dispose();
		_sqliteConn?.Dispose();
		if ( _tempDir is not null )
		{
			try { Directory.Delete(_tempDir, recursive: true); } catch { }
		}
	}

	// ══════════════════════════════════════════════════════════════════════════
	// Scenario A: Syncthing → Starsky (receive a file)
	// ══════════════════════════════════════════════════════════════════════════

	[TestMethod]
	public async Task SyncthingToStarsky_FileIsDownloaded_WhenSyncthingHasFile()
	{
		var syncthingPath = FindSyncthing();
		if ( syncthingPath is null )
		{
			Assert.Inconclusive("syncthing binary not found on PATH — skipping integration test.");
		}

		SetupDirs(out var homeDir, out var syncDir, out var localDir);
		SetupPorts(out var bepPort, out var guiPort);
		SetupCert(out var localCert, out var testRsaKey, out var ourDeviceId);

		// Pre-populate Syncthing's folder so it indexes the file on startup
		const string testContent = "hello from syncthing\n";
		File.WriteAllText(Path.Combine(syncDir, "receive-test.txt"), testContent);

		RunToCompletion(syncthingPath!, "generate", "--home", homeDir);
		var apiKey = PatchConfigPortsOnly(homeDir, bepPort, guiPort);

		_syncthingProcess = StartSyncthing(syncthingPath!, homeDir);
		await WaitForRestApiAsync(guiPort, apiKey);

		var syncthingDeviceId = await GetSyncthingDeviceIdAsync(guiPort, apiKey);
		await ConfigureDeviceAndFolderViaApiAsync(
			guiPort, apiKey, syncDir, FolderId, syncthingDeviceId, ourDeviceId);

		// Wait for Syncthing to scan the folder and index our test file
		await WaitForFolderReadyAsync(guiPort, apiKey, FolderId);

		// Wire up SyncEngine
		var fileDownloadedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		using var cts = new CancellationTokenSource();
		var (cm, engine) = BuildEngine(localCert, localDir, _db!,
			onFileDownloaded: (_, _) => { fileDownloadedTcs.TrySetResult(); return Task.CompletedTask; });

		await engine.StartAsync(cts.Token);
		await ConnectToPeerAsync(cm, localCert, testRsaKey, syncthingDeviceId, bepPort, cts.Token);

		// Wait for the file to arrive
		var done = await Task.WhenAny(fileDownloadedTcs.Task, Task.Delay(SyncTimeout));

		await engine.StopAsync(CancellationToken.None);
		cts.Cancel();
		cm.Dispose();
		engine.Dispose();
		testRsaKey.Dispose();
		localCert.Dispose();

		Assert.AreEqual(fileDownloadedTcs.Task, done,
			"File was not downloaded within the timeout.");

		var destPath = Path.Combine(localDir, "receive-test.txt");
		Assert.IsTrue(File.Exists(destPath), $"Expected downloaded file at {destPath}.");
		Assert.AreEqual(testContent, File.ReadAllText(destPath));
	}

	// ══════════════════════════════════════════════════════════════════════════
	// Scenario B: Starsky → Syncthing (send a file)
	// ══════════════════════════════════════════════════════════════════════════

	[TestMethod]
	public async Task StarskyToSyncthing_FileIsReceived_WhenLocalFileIsAdded()
	{
		var syncthingPath = FindSyncthing();
		if ( syncthingPath is null )
		{
			Assert.Inconclusive("syncthing binary not found on PATH — skipping integration test.");
		}

		SetupDirs(out var homeDir, out var syncDir, out var localDir);
		SetupPorts(out var bepPort, out var guiPort);
		SetupCert(out var localCert, out var testRsaKey, out var ourDeviceId);

		// Pre-populate our local folder with the file we want to send
		const string sendContent = "hello from starsky\n";
		File.WriteAllText(Path.Combine(localDir, "send-test.txt"), sendContent);

		RunToCompletion(syncthingPath!, "generate", "--home", homeDir);
		var apiKey = PatchConfigPortsOnly(homeDir, bepPort, guiPort);

		_syncthingProcess = StartSyncthing(syncthingPath!, homeDir);
		await WaitForRestApiAsync(guiPort, apiKey);

		var syncthingDeviceId = await GetSyncthingDeviceIdAsync(guiPort, apiKey);
		await ConfigureDeviceAndFolderViaApiAsync(
			guiPort, apiKey, syncDir, FolderId, syncthingDeviceId, ourDeviceId);

		// Allow Syncthing to apply the new folder config and become idle
		await WaitForFolderReadyAsync(guiPort, apiKey, FolderId);

		// Scan local folder to populate ConnectFileMeta + ConnectBlockInfo
		var folderModel = new FolderModel(_db!);
		using var scanner = new Scanner(
			_db!, [localDir], new NullSyncLocalChangeSource(),
			folderModel, NullLogger<Scanner>.Instance);
		await scanner.ScanFolderDirectAsync(FolderId, localDir);

		// Wire up SyncEngine
		using var cts = new CancellationTokenSource();
		var (cm, engine) = BuildEngine(localCert, localDir, _db!, onFileDownloaded: null);

		await engine.StartAsync(cts.Token);
		await ConnectToPeerAsync(cm, localCert, testRsaKey, syncthingDeviceId, bepPort, cts.Token);

		// Poll for the file to appear in Syncthing's folder
		var destPath = Path.Combine(syncDir, "send-test.txt");
		var deadline = DateTimeOffset.UtcNow + SyncTimeout;
		while ( DateTimeOffset.UtcNow < deadline && !File.Exists(destPath) )
		{
			await Task.Delay(500);
		}

		await engine.StopAsync(CancellationToken.None);
		cts.Cancel();
		cm.Dispose();
		engine.Dispose();
		scanner.Dispose();
		testRsaKey.Dispose();
		localCert.Dispose();

		Assert.IsTrue(File.Exists(destPath),
			$"Expected Syncthing to have written the file at {destPath}.");
		Assert.AreEqual(sendContent, File.ReadAllText(destPath));
	}

	// ── Engine factory ────────────────────────────────────────────────────────

	private (ConnectionManager Cm, SyncEngine Engine) BuildEngine(
		X509Certificate2 localCert,
		string localDir,
		ApplicationDbContext db,
		Func<string, string, Task>? onFileDownloaded)
	{
		var localDeviceIdBytes = DeviceIdentity.DeriveDeviceIdBytes(localCert);
		var localDeviceId = DeviceIdentity.NormalizeDeviceId(DeviceIdentity.DeriveDeviceId(localCert));
		var folderRoots = new Dictionary<string, string> { [FolderId] = localDir };

		// listenPort: 0 lets the OS pick any free port, avoiding conflicts when tests run in parallel
		var cm = new ConnectionManager(localCert, NullLogger<ConnectionManager>.Instance, listenPort: 0);
		var folderModel = new FolderModel(db);
		var blockStore = new BlockStore();
		var downloader = new Downloader(cm, blockStore, NullLogger<Downloader>.Instance);
		var resolver = new ConflictResolver();

		var engine = new SyncEngine(
			cm, folderModel, blockStore, downloader, resolver,
			folderRoots, localDeviceIdBytes, localDeviceId,
			NullLogger<SyncEngine>.Instance);

		engine.OnFileDownloaded = onFileDownloaded;
		return (cm, engine);
	}

	// ── Connection helpers ────────────────────────────────────────────────────

	/// <summary>
	/// Establishes a BEP connection to Syncthing via TlsDialer (Linux/CI) or
	/// Homebrew openssl s_client (macOS SecureTransport limitation) and injects
	/// the resulting stream into the ConnectionManager.
	/// </summary>
	private async Task ConnectToPeerAsync(
		ConnectionManager cm,
		X509Certificate2 localCert,
		RSA testRsaKey,
		string peerDeviceId,
		int bepPort,
		CancellationToken ct)
	{
		var normalizedPeerId = DeviceIdentity.NormalizeDeviceId(peerDeviceId);
		var peerDeviceIdBytes = Base32.Decode(normalizedPeerId[..52]);

		Stream stream;
		try
		{
			var allowedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { peerDeviceId };
			var dialer = new TlsDialer(localCert, allowedIds);
			stream = await dialer.ConnectAsync("127.0.0.1", bepPort, ct);
		}
		catch ( System.Security.Authentication.AuthenticationException ex )
			when ( ex.Message.Contains("protocol", StringComparison.OrdinalIgnoreCase) ||
			       ex.InnerException?.Message.Contains("protocol", StringComparison.OrdinalIgnoreCase) == true )
		{
			var opensslBin = FindOpenSslBinary();
			if ( opensslBin is null )
			{
				Assert.Inconclusive(
					"TLS 1.3 negotiation failed (macOS SecureTransport) and no Homebrew OpenSSL found. " +
					"Install via: brew install openssl@3");
				return;
			}

			stream = await ConnectViaOpenSslAsync(opensslBin, "127.0.0.1", bepPort,
				localCert, testRsaKey, _tempDir!);
		}

		await cm.InjectConnectionAsync(stream, normalizedPeerId, peerDeviceIdBytes, ct);
	}

	private static async Task<Stream> ConnectViaOpenSslAsync(
		string opensslBin, string host, int port,
		X509Certificate2 cert, RSA privateKey, string tempDir)
	{
		var certFile = Path.Combine(tempDir, "openssl-client-cert.pem");
		var keyFile = Path.Combine(tempDir, "openssl-client-key.pem");

		File.WriteAllText(certFile, cert.ExportCertificatePem());
		File.WriteAllText(keyFile, privateKey.ExportPkcs8PrivateKeyPem());

		var psi = new ProcessStartInfo(opensslBin)
		{
			UseShellExecute = false,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		psi.ArgumentList.Add("s_client");
		psi.ArgumentList.Add("-tls1_3");
		psi.ArgumentList.Add("-connect"); psi.ArgumentList.Add($"{host}:{port}");
		psi.ArgumentList.Add("-cert"); psi.ArgumentList.Add(certFile);
		psi.ArgumentList.Add("-key"); psi.ArgumentList.Add(keyFile);
		psi.ArgumentList.Add("-servername"); psi.ArgumentList.Add("syncthing");
		psi.ArgumentList.Add("-quiet");
		psi.ArgumentList.Add("-ign_eof");

		var proc = Process.Start(psi)!;
		await Task.Delay(1000);

		if ( proc.HasExited )
		{
			var err = await proc.StandardError.ReadToEndAsync();
			throw new InvalidOperationException(
				$"openssl s_client exited before handshake (code {proc.ExitCode}): {err}");
		}

		return new OpenSslPipeStream(proc);
	}

	private static string? FindOpenSslBinary()
	{
		foreach ( var c in new[]
		{
			"/opt/homebrew/bin/openssl",
			"/opt/homebrew/opt/openssl@3/bin/openssl",
			"/usr/local/bin/openssl",
			"/usr/local/opt/openssl@3/bin/openssl",
		} )
		{
			if ( File.Exists(c) ) return c;
		}

		return null;
	}

	// ── Test setup helpers ────────────────────────────────────────────────────

	private void SetupDirs(out string homeDir, out string syncDir, out string localDir)
	{
		_tempDir = Path.Combine(Path.GetTempPath(),
			"starsky-sync-integration-" + Guid.NewGuid().ToString("N"));
		homeDir = Path.Combine(_tempDir, "syncthing-home");
		syncDir = Path.Combine(_tempDir, "sync");
		localDir = Path.Combine(_tempDir, "local");
		Directory.CreateDirectory(homeDir);
		Directory.CreateDirectory(syncDir);
		Directory.CreateDirectory(localDir);
		// Syncthing requires a .stfolder marker in managed folders
		File.WriteAllText(Path.Combine(syncDir, ".stfolder"), string.Empty);

		_sqliteConn = new SqliteConnection("Filename=:memory:");
		_sqliteConn.Open();
		var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite(_sqliteConn)
			.Options;
		_db = new ApplicationDbContext(dbOptions);
		_db.Database.EnsureCreated();
	}

	private static void SetupPorts(out int bepPort, out int guiPort)
	{
		bepPort = FindFreePort();
		guiPort = FindFreePort();
	}

	private void SetupCert(out X509Certificate2 localCert, out RSA testRsaKey, out string ourDeviceId)
	{
		// Keep raw RSA so the openssl fallback can export it without macOS Keychain restriction
		testRsaKey = RSA.Create(2048);
		var req = new CertificateRequest(
			new X500DistinguishedName("CN=syncthing"),
			testRsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		var now = DateTimeOffset.UtcNow;
		using var selfSigned = req.CreateSelfSigned(now.AddDays(-1), now.AddYears(1));
		localCert = X509CertificateLoader.LoadPkcs12(selfSigned.Export(X509ContentType.Pfx), null);
		ourDeviceId = DeviceIdentity.NormalizeDeviceId(DeviceIdentity.DeriveDeviceId(localCert));
	}

	// ── Syncthing config (ports only — device/folder added via REST API) ─────

	private static string PatchConfigPortsOnly(string homeDir, int bepPort, int guiPort)
	{
		var configPath = Path.Combine(homeDir, "config.xml");
		var doc = XDocument.Load(configPath);
		var conf = doc.Root!;
		string apiKey = string.Empty;

		var gui = conf.Element("gui");
		if ( gui is not null )
		{
			var addr = gui.Element("address");
			if ( addr is not null ) addr.Value = $"127.0.0.1:{guiPort}";
			apiKey = gui.Element("apikey")?.Value ?? string.Empty;
		}

		var options = conf.Element("options");
		if ( options is not null )
		{
			options.Elements("listenAddress").Remove();
			options.Add(new XElement("listenAddress", $"tcp://127.0.0.1:{bepPort}"));
			SetOrAdd(options, "globalAnnounceEnabled", "false");
			SetOrAdd(options, "localAnnounceEnabled", "false");
			SetOrAdd(options, "relaysEnabled", "false");
			SetOrAdd(options, "natEnabled", "false");
			SetOrAdd(options, "startBrowser", "false");
		}

		doc.Save(configPath);
		return apiKey;
	}

	private static void SetOrAdd(XElement parent, string name, string value)
	{
		var el = parent.Element(name);
		if ( el is not null ) el.Value = value;
		else parent.Add(new XElement(name, value));
	}

	// ── REST API configuration ────────────────────────────────────────────────

	private static async Task<string> GetSyncthingDeviceIdAsync(int guiPort, string apiKey)
	{
		using var http = BuildApiClient(guiPort, apiKey);
		var json = await http.GetStringAsync($"http://127.0.0.1:{guiPort}/rest/system/status");
		var node = JsonNode.Parse(json)!;
		return node["myID"]!.GetValue<string>();
	}

	/// <summary>
	/// Adds our device and a shared folder to Syncthing's config via the REST API.
	/// This is version-independent and avoids XML format guessing.
	/// </summary>
	private static async Task ConfigureDeviceAndFolderViaApiAsync(
		int guiPort,
		string apiKey,
		string syncDir,
		string folderId,
		string syncthingDeviceId,
		string ourDeviceId)
	{
		using var http = BuildApiClient(guiPort, apiKey);
		var base_ = $"http://127.0.0.1:{guiPort}";

		// Add our device as a known peer
		var deviceJson = $$"""
		{
			"deviceID": "{{ourDeviceId}}",
			"name": "starsky-integration-test",
			"addresses": ["dynamic"],
			"compression": "metadata",
			"paused": false,
			"autoAcceptFolders": false
		}
		""";
		var deviceResp = await http.PostAsync($"{base_}/rest/config/devices",
			new StringContent(deviceJson, Encoding.UTF8, "application/json"));
		deviceResp.EnsureSuccessStatusCode();

		// Add the shared folder with both devices
		var escapedPath = syncDir.Replace("\\", "\\\\");
		var folderJson = $$"""
		{
			"id": "{{folderId}}",
			"label": "Starsky Integration Test",
			"path": "{{escapedPath}}",
			"type": "sendreceive",
			"rescanIntervalS": 5,
			"fsWatcherEnabled": true,
			"fsWatcherDelayS": 1,
			"devices": [
				{"deviceID": "{{syncthingDeviceId}}", "introducedBy": ""},
				{"deviceID": "{{ourDeviceId}}", "introducedBy": ""}
			],
			"paused": false,
			"markerName": ".stfolder"
		}
		""";
		var folderResp = await http.PostAsync($"{base_}/rest/config/folders",
			new StringContent(folderJson, Encoding.UTF8, "application/json"));
		folderResp.EnsureSuccessStatusCode();
	}

	/// <summary>
	/// Waits until the folder is scanned and in the "idle" state.
	/// Throws if the folder is not found or doesn't become idle within the timeout.
	/// </summary>
	private static async Task WaitForFolderReadyAsync(int guiPort, string apiKey, string folderId)
	{
		using var http = BuildApiClient(guiPort, apiKey);
		var url = $"http://127.0.0.1:{guiPort}/rest/db/status?folder={folderId}";
		var deadline = DateTimeOffset.UtcNow + StartupTimeout;

		while ( DateTimeOffset.UtcNow < deadline )
		{
			try
			{
				var resp = await http.GetAsync(url);
				if ( resp.IsSuccessStatusCode )
				{
					var json = await resp.Content.ReadAsStringAsync();
					var node = JsonNode.Parse(json);
					var state = node?["state"]?.GetValue<string>();
					if ( state is "idle" or "localWaiting" or "syncing" )
					{
						return;
					}
				}
			}
			catch { /* retry */ }

			await Task.Delay(500);
		}

		throw new TimeoutException(
			$"Syncthing folder '{folderId}' did not become ready within {StartupTimeout.TotalSeconds} s.");
	}

	private static HttpClient BuildApiClient(int guiPort, string apiKey)
	{
		var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
		http.DefaultRequestHeaders.Add("X-API-Key", apiKey);
		return http;
	}

	// ── Process helpers ───────────────────────────────────────────────────────

	private static string? FindSyncthing()
	{
		var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
		foreach ( var dir in pathVar.Split(Path.PathSeparator) )
		{
			foreach ( var name in new[] { "syncthing", "syncthing.exe" } )
			{
				var c = Path.Combine(dir, name);
				if ( File.Exists(c) ) return c;
			}
		}

		foreach ( var c in new[]
		{
			"/usr/local/bin/syncthing",
			"/opt/homebrew/bin/syncthing",
			"/Applications/Syncthing.app/Contents/Resources/syncthing/syncthing",
		} )
		{
			if ( File.Exists(c) ) return c;
		}

		return null;
	}

	private static int FindFreePort()
	{
		using var l = new TcpListener(System.Net.IPAddress.Loopback, 0);
		l.Start();
		var port = ( ( System.Net.IPEndPoint )l.LocalEndpoint ).Port;
		l.Stop();
		return port;
	}

	private static void RunToCompletion(string syncthingPath, params string[] args)
	{
		var psi = new ProcessStartInfo(syncthingPath)
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
		};
		foreach ( var arg in args ) psi.ArgumentList.Add(arg);
		using var proc = Process.Start(psi)!;
		proc.WaitForExit(TimeSpan.FromSeconds(30));
		if ( proc.ExitCode != 0 )
		{
			throw new InvalidOperationException(
				$"syncthing {string.Join(' ', args)} exited with code {proc.ExitCode}.");
		}
	}

	private static Process StartSyncthing(string syncthingPath, string homeDir)
	{
		var psi = new ProcessStartInfo(syncthingPath)
		{
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		psi.ArgumentList.Add("serve");
		psi.ArgumentList.Add("--home"); psi.ArgumentList.Add(homeDir);
		psi.ArgumentList.Add("--no-browser");
		return Process.Start(psi)!;
	}

	private static async Task WaitForRestApiAsync(int guiPort, string apiKey)
	{
		using var http = BuildApiClient(guiPort, apiKey);
		var url = $"http://127.0.0.1:{guiPort}/rest/system/ping";
		var deadline = DateTimeOffset.UtcNow + StartupTimeout;

		while ( DateTimeOffset.UtcNow < deadline )
		{
			try
			{
				var response = await http.GetAsync(url);
				if ( response.IsSuccessStatusCode ) return;
			}
			catch ( Exception ex ) when ( ex is HttpRequestException or TaskCanceledException ) { }

			await Task.Delay(500);
		}

		throw new TimeoutException(
			$"Syncthing REST API did not become ready within {StartupTimeout.TotalSeconds} s.");
	}

	// ── OpenSslPipeStream ─────────────────────────────────────────────────────

	private sealed class OpenSslPipeStream : Stream
	{
		private readonly Process _process;
		private readonly Stream _read;
		private readonly Stream _write;

		public OpenSslPipeStream(Process process)
		{
			_process = process;
			_read = process.StandardOutput.BaseStream;
			_write = process.StandardInput.BaseStream;
		}

		public override bool CanRead => true;
		public override bool CanWrite => true;
		public override bool CanSeek => false;
		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override void Flush() => _write.Flush();
		public override Task FlushAsync(CancellationToken ct) => _write.FlushAsync(ct);
		public override int Read(byte[] buf, int off, int count) => _read.Read(buf, off, count);
		public override Task<int> ReadAsync(byte[] buf, int off, int count, CancellationToken ct) => _read.ReadAsync(buf, off, count, ct);
		public override ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default) => _read.ReadAsync(buf, ct);
		public override void Write(byte[] buf, int off, int count) => _write.Write(buf, off, count);
		public override Task WriteAsync(byte[] buf, int off, int count, CancellationToken ct) => _write.WriteAsync(buf, off, count, ct);
		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default) => _write.WriteAsync(buf, ct);
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();

		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				try { _process.Kill(entireProcessTree: true); } catch { }
				_process.Dispose();
			}

			base.Dispose(disposing);
		}

		public override async ValueTask DisposeAsync()
		{
			try { _process.Kill(entireProcessTree: true); } catch { }
			_process.Dispose();
			await base.DisposeAsync();
		}
	}

	// ── Null change source ────────────────────────────────────────────────────

	private sealed class NullSyncLocalChangeSource : ISyncLocalChangeSource
	{
#pragma warning disable CS0067
		public event EventHandler<SyncLocalChangeEventArgs>? LocalFileChanged;
#pragma warning restore CS0067
	}
}
