using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using starsky.foundation.connect.Crypto;

namespace starsky.foundation.connect.Transport;

/// <summary>
/// Listens for inbound BEP mTLS connections on TCP port 22000 (default).
/// </summary>
public sealed class TlsListener : IDisposable
{
	private readonly X509Certificate2 _serverCert;
	private readonly IReadOnlySet<string> _allowedDeviceIds;
	private readonly ILogger<TlsListener> _logger;
	private readonly TcpListener _tcpListener;

	public TlsListener(
		X509Certificate2 serverCert,
		IReadOnlySet<string> allowedDeviceIds,
		ILogger<TlsListener> logger,
		int port = 22000)
	{
		_serverCert = serverCert;
		_allowedDeviceIds = allowedDeviceIds;
		_logger = logger;
		_tcpListener = new TcpListener(IPAddress.Any, port);
	}

	public void Start() => _tcpListener.Start();

	public void Stop() => _tcpListener.Stop();

	/// <summary>
	/// Accepts one inbound TLS connection and validates the peer.
	/// Returns the authenticated <see cref="SslStream"/> or null if authentication fails.
	/// </summary>
	public async Task<SslStream?> AcceptAsync(CancellationToken ct = default)
	{
		var tcp = await _tcpListener.AcceptTcpClientAsync(ct);

		var ssl = new SslStream(
			tcp.GetStream(),
			leaveInnerStreamOpen: false,
			userCertificateValidationCallback: ValidatePeerCertificate);

		try
		{
			var options = new SslServerAuthenticationOptions
			{
				ServerCertificate = _serverCert,
				ClientCertificateRequired = true,
				EnabledSslProtocols = SslProtocols.Tls13,
				CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
				RemoteCertificateValidationCallback = ValidatePeerCertificate,
			};

			await ssl.AuthenticateAsServerAsync(options, ct);
			return ssl;
		}
		catch ( AuthenticationException ex )
		{
			_logger.LogWarning(ex, "Inbound TLS authentication failed.");
			await ssl.DisposeAsync();
			return null;
		}
	}

	private bool ValidatePeerCertificate(
		object sender,
		X509Certificate? certificate,
		X509Chain? chain,
		SslPolicyErrors sslPolicyErrors)
	{
		if ( certificate is null )
		{
			return false;
		}

		var cert2 = certificate as X509Certificate2 ?? new X509Certificate2(certificate);
		var peerId = DeviceIdentity.DeriveDeviceId(cert2);
		var peerIdNormalized = DeviceIdentity.NormalizeDeviceId(peerId);

		return _allowedDeviceIds.Contains(peerId) ||
		       _allowedDeviceIds.Contains(peerIdNormalized);
	}

	public void Dispose() => _tcpListener.Stop();
}
