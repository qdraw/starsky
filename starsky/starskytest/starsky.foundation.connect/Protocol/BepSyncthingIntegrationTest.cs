using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Crypto;
using starsky.foundation.connect.Protocol;
using starsky.foundation.connect.Protocol.Generated;
using starsky.foundation.connect.Transport;

namespace starskytest.starsky.foundation.connect.Protocol;

/// <summary>
/// Integration test that boots a real Syncthing process and verifies that our BEP
/// implementation can exchange Hello + ClusterConfig with it on the wire.
///
/// Requires the `syncthing` binary on PATH. Skips (Inconclusive) when not found.
/// Runs on dynamic ports to avoid conflicts with any already-running Syncthing instance.
/// On macOS, falls back to Homebrew's openssl s_client when .NET's SecureTransport
/// cannot negotiate TLS 1.3.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public sealed class BepSyncthingIntegrationTest
{
	private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(30);

	private string? _tempDir;
	private Process? _syncthingProcess;

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			_syncthingProcess?.Kill(entireProcessTree: true);
			_syncthingProcess?.Dispose();
		}
		catch { /* best effort */ }

		if ( _tempDir is not null )
		{
			try { Directory.Delete(_tempDir, recursive: true); }
			catch { /* best effort */ }
		}
	}

	[TestMethod]
	public async Task HelloAndClusterConfig_RoundTrip_WithRealSyncthing()
	{
		var syncthingPath = FindSyncthing();
		if ( syncthingPath is null )
		{
			Assert.Inconclusive("syncthing binary not found on PATH — skipping integration test.");
		}

		var bepPort = FindFreePort();
		var guiPort = FindFreePort();

		_tempDir = Path.Combine(Path.GetTempPath(), "starsky-bep-integration-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_tempDir);

		// ── Identity cert ──────────────────────────────────────────────────────
		// Generate once before Syncthing starts so we can register our Device ID in config.
		// Keep the raw RSA key object (not via GetRSAPrivateKey on a keychain-backed cert)
		// so the openssl fallback path can export it as PEM without OS restrictions.
		using var testRsaKey = RSA.Create(2048);
		X509Certificate2 localCert;
		{
			var req = new CertificateRequest(
				new X500DistinguishedName("CN=syncthing"),
				testRsaKey,
				HashAlgorithmName.SHA256,
				RSASignaturePadding.Pkcs1);
			var now = DateTimeOffset.UtcNow;
			using var selfSigned = req.CreateSelfSigned(now.AddDays(-1), now.AddYears(1));
			// LoadPkcs12 is required for SslStream on macOS (stores key in Keychain)
			localCert = X509CertificateLoader.LoadPkcs12(selfSigned.Export(X509ContentType.Pfx), null);
		}

		var ourDeviceId = DeviceIdentity.NormalizeDeviceId(DeviceIdentity.DeriveDeviceId(localCert));

		// ── Syncthing setup ───────────────────────────────────────────────────
		RunToCompletion(syncthingPath!, "generate", "--home", _tempDir!);

		// Patch config: dynamic ports + register our device so Syncthing doesn't
		// close the connection after Hello (it closes unknown devices immediately).
		var apiKey = PatchConfig(_tempDir!, bepPort, guiPort, ourDeviceId);

		var peerDeviceId = ReadSyncthingDeviceId(_tempDir!);
		_syncthingProcess = StartSyncthing(syncthingPath!, _tempDir!);
		await WaitForRestApiAsync(guiPort, apiKey);

		// ── Transport ─────────────────────────────────────────────────────────
		var allowedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { peerDeviceId };
		var dialer = new TlsDialer(localCert, allowedIds);

		Stream bepTransport;
		try
		{
			bepTransport = await dialer.ConnectAsync("127.0.0.1", bepPort, CancellationToken.None);
		}
		catch ( System.Security.Authentication.AuthenticationException ex )
			when ( ex.Message.Contains("protocol", StringComparison.OrdinalIgnoreCase) ||
			       ex.InnerException?.Message.Contains("protocol", StringComparison.OrdinalIgnoreCase) == true )
		{
			// macOS SecureTransport (.NET) falls back to TLS 1.2; Syncthing v2 requires TLS 1.3.
			// Try Homebrew's openssl which uses OpenSSL 3.x with proper TLS 1.3 support.
			var opensslBin = FindOpenSslBinary();
			if ( opensslBin is null )
			{
				Assert.Inconclusive(
					"TLS 1.3 negotiation failed (macOS SecureTransport limitation) and no Homebrew " +
					"OpenSSL found for fallback. Install via: brew install openssl@3");
				return;
			}

			// testRsaKey is still in scope and owned by us — export works without keychain restriction
			bepTransport = await ConnectViaOpenSslAsync(opensslBin, "127.0.0.1", bepPort,
				localCert, testRsaKey, _tempDir!);
		}

		await using ( bepTransport )
		{
			await DoBepExchangeAsync(bepTransport);
		}
	}

	// ── BEP exchange ──────────────────────────────────────────────────────────

	private static async Task DoBepExchangeAsync(Stream transport)
	{
		var writer = new BepWriter(transport);
		var reader = new BepReader(transport);

		// ── Hello exchange ────────────────────────────────────────────────────

		var ourHello = new Hello
		{
			DeviceName = "starsky-integration-test",
			ClientName = "starsky-connect",
			ClientVersion = "v0.9.5",
		};

		await writer.WriteHelloAsync(ourHello, CancellationToken.None);
		var theirHello = await reader.ReadHelloAsync(CancellationToken.None);

		Assert.AreEqual("syncthing", theirHello.ClientName,
			"Expected ClientName to be 'syncthing' in Syncthing's Hello.");

		// ── ClusterConfig exchange ────────────────────────────────────────────

		var ourConfig = new ClusterConfig();
		await writer.WriteMessageAsync(ourConfig, MessageType.ClusterConfig, compress: false, CancellationToken.None);

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
		var msg = await reader.ReadMessageAsync(cts.Token);

		Assert.AreEqual(MessageType.ClusterConfig, msg.Header.Type,
			$"Expected ClusterConfig as the first post-Hello message, got {msg.Header.Type}.");

		var theirConfig = msg.ParseBody(ClusterConfig.Parser);
		Assert.IsNotNull(theirConfig,
			"Expected a parseable ClusterConfig body from Syncthing.");
	}

	// ── openssl TLS 1.3 fallback ──────────────────────────────────────────────

	/// <summary>
	/// Connects via Homebrew's openssl s_client using TLS 1.3, presenting <paramref name="cert"/>
	/// (public part) with <paramref name="privateKey"/> as the client certificate.
	/// Returns a bidirectional <see cref="Stream"/> over the process's stdin/stdout.
	/// </summary>
	private static async Task<Stream> ConnectViaOpenSslAsync(
		string opensslBin,
		string host,
		int port,
		X509Certificate2 cert,
		RSA privateKey,
		string tempDir)
	{
		var certFile = Path.Combine(tempDir, "openssl-client-cert.pem");
		var keyFile = Path.Combine(tempDir, "openssl-client-key.pem");

		// cert.ExportCertificatePem() exports only the public cert (no OS export restriction)
		File.WriteAllText(certFile, cert.ExportCertificatePem());
		// privateKey is our own RSA object (not keychain-backed) so export is allowed on macOS
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
		psi.ArgumentList.Add("-connect");
		psi.ArgumentList.Add($"{host}:{port}");
		psi.ArgumentList.Add("-cert");
		psi.ArgumentList.Add(certFile);
		psi.ArgumentList.Add("-key");
		psi.ArgumentList.Add(keyFile);
		psi.ArgumentList.Add("-servername");
		psi.ArgumentList.Add("syncthing");
		// -quiet suppresses session/cert diagnostic output; data flows clean on stdin/stdout.
		// -ign_eof keeps the process alive even if stdin reaches EOF prematurely.
		psi.ArgumentList.Add("-quiet");
		psi.ArgumentList.Add("-ign_eof");

		var proc = Process.Start(psi)!;

		// Wait for the TLS handshake to complete before returning the stream.
		// During this delay, Syncthing sends its Hello into the pipe buffer.
		await Task.Delay(1000);

		if ( proc.HasExited )
		{
			var err = await proc.StandardError.ReadToEndAsync();
			throw new InvalidOperationException(
				$"openssl s_client exited before handshake completed (code {proc.ExitCode}): {err}");
		}

		return new OpenSslPipeStream(proc);
	}

	/// <summary>
	/// Finds Homebrew's OpenSSL binary (OpenSSL 3.x, not macOS's LibreSSL).
	/// Returns null if none is found.
	/// </summary>
	private static string? FindOpenSslBinary()
	{
		var candidates = new[]
		{
			"/opt/homebrew/bin/openssl",               // Apple Silicon Homebrew
			"/opt/homebrew/opt/openssl@3/bin/openssl", // explicit @3 formula
			"/usr/local/bin/openssl",                   // Intel Mac Homebrew
			"/usr/local/opt/openssl@3/bin/openssl",
		};

		foreach ( var candidate in candidates )
		{
			if ( File.Exists(candidate) )
			{
				return candidate;
			}
		}

		return null;
	}

	/// <summary>
	/// Bidirectional stream over an openssl s_client subprocess's stdin/stdout.
	/// Reading reads from the TLS tunnel (server → us); writing writes to it (us → server).
	/// </summary>
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

		public override Task FlushAsync(CancellationToken cancellationToken) =>
			_write.FlushAsync(cancellationToken);

		public override int Read(byte[] buffer, int offset, int count) =>
			_read.Read(buffer, offset, count);

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
			_read.ReadAsync(buffer, offset, count, cancellationToken);

		public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
			_read.ReadAsync(buffer, cancellationToken);

		public override void Write(byte[] buffer, int offset, int count) =>
			_write.Write(buffer, offset, count);

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
			_write.WriteAsync(buffer, offset, count, cancellationToken);

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
			_write.WriteAsync(buffer, cancellationToken);

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

	// ── Helpers ───────────────────────────────────────────────────────────────

	private static string? FindSyncthing()
	{
		var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
		var candidates = pathVar.Split(Path.PathSeparator)
			.Select(dir => Path.Combine(dir, "syncthing"))
			.Concat([
				"/usr/local/bin/syncthing",
				"/opt/homebrew/bin/syncthing",
				"/Applications/Syncthing.app/Contents/Resources/syncthing/syncthing",
			]);

		foreach ( var candidate in candidates )
		{
			if ( File.Exists(candidate) )
			{
				return candidate;
			}
		}

		foreach ( var candidate in pathVar.Split(Path.PathSeparator)
			.Select(dir => Path.Combine(dir, "syncthing.exe")) )
		{
			if ( File.Exists(candidate) )
			{
				return candidate;
			}
		}

		return null;
	}

	private static int FindFreePort()
	{
		using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
		listener.Start();
		var port = ( ( System.Net.IPEndPoint )listener.LocalEndpoint ).Port;
		listener.Stop();
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
		foreach ( var arg in args )
		{
			psi.ArgumentList.Add(arg);
		}

		using var proc = Process.Start(psi)!;
		proc.WaitForExit(TimeSpan.FromSeconds(30));
		if ( proc.ExitCode != 0 )
		{
			throw new InvalidOperationException(
				$"syncthing {string.Join(' ', args)} exited with code {proc.ExitCode}.");
		}
	}

	/// <summary>
	/// Patches config.xml: sets dynamic BEP/GUI ports, disables discovery/relay,
	/// and adds <paramref name="ourDeviceId"/> as a known device so Syncthing doesn't
	/// close the BEP connection immediately after the Hello exchange.
	/// Returns the GUI API key.
	/// </summary>
	private static string PatchConfig(string homeDir, int bepPort, int guiPort, string ourDeviceId)
	{
		var configPath = Path.Combine(homeDir, "config.xml");
		var doc = XDocument.Load(configPath);
		var conf = doc.Root!;

		string apiKey = string.Empty;

		var gui = conf.Element("gui");
		if ( gui is not null )
		{
			var addrEl = gui.Element("address");
			if ( addrEl is not null )
			{
				addrEl.Value = $"127.0.0.1:{guiPort}";
			}

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

		// Add our device as a known peer so Syncthing keeps the BEP connection
		// open past the Hello exchange (Syncthing closes connections from unknown devices).
		conf.Add(new XElement("device",
			new XAttribute("id", ourDeviceId),
			new XAttribute("name", "starsky-integration-test"),
			new XAttribute("compression", "metadata"),
			new XAttribute("introducer", "false"),
			new XAttribute("skipIntroductionRemovals", "false"),
			new XAttribute("encryptionPassword", ""),
			new XElement("address", "dynamic"),
			new XElement("paused", "false"),
			new XElement("autoAcceptFolders", "false"),
			new XElement("maxSendKbps", "0"),
			new XElement("maxRecvKbps", "0"),
			new XElement("maxRequestKiB", "0"),
			new XElement("untrusted", "false"),
			new XElement("remoteGUIPort", "0"),
			new XElement("numConnections", "0")));

		doc.Save(configPath);
		return apiKey;
	}

	private static void SetOrAdd(XElement parent, string name, string value)
	{
		var el = parent.Element(name);
		if ( el is not null )
		{
			el.Value = value;
		}
		else
		{
			parent.Add(new XElement(name, value));
		}
	}

	private static string ReadSyncthingDeviceId(string homeDir)
	{
		var certPem = File.ReadAllText(Path.Combine(homeDir, "cert.pem"));
		var cert = X509Certificate2.CreateFromPem(certPem);
		return DeviceIdentity.NormalizeDeviceId(DeviceIdentity.DeriveDeviceId(cert));
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
		psi.ArgumentList.Add("--home");
		psi.ArgumentList.Add(homeDir);
		psi.ArgumentList.Add("--no-browser");

		return Process.Start(psi)!;
	}

	private static async Task WaitForRestApiAsync(int guiPort, string apiKey)
	{
		using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
		http.DefaultRequestHeaders.Add("X-API-Key", apiKey);
		var url = $"http://127.0.0.1:{guiPort}/rest/system/ping";
		var deadline = DateTimeOffset.UtcNow + StartupTimeout;

		while ( DateTimeOffset.UtcNow < deadline )
		{
			try
			{
				var response = await http.GetAsync(url);
				if ( response.IsSuccessStatusCode )
				{
					return;
				}
			}
			catch ( Exception ex ) when ( ex is HttpRequestException or TaskCanceledException ) { }

			await Task.Delay(500);
		}

		throw new TimeoutException(
			$"Syncthing REST API at {url} did not become ready within {StartupTimeout.TotalSeconds} s.");
	}
}
