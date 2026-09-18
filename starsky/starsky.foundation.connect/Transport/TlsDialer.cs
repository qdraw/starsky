using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.connect.Crypto;

namespace starsky.foundation.connect.Transport;

/// <summary>
/// Establishes outbound mTLS 1.3 connections and validates peers by Device ID.
/// </summary>
public sealed class TlsDialer
{
	private readonly X509Certificate2 _localCert;
	private readonly IReadOnlySet<string> _allowedDeviceIds;

	public TlsDialer(X509Certificate2 localCert, IReadOnlySet<string> allowedDeviceIds)
	{
		_localCert = localCert;
		_allowedDeviceIds = allowedDeviceIds;
	}

	/// <summary>
	/// Opens a TLS 1.3 connection to <paramref name="host"/>:<paramref name="port"/> and
	/// validates the peer's certificate against the Device ID allow-list.
	/// </summary>
	public async Task<SslStream> ConnectAsync(
		string host,
		int port,
		CancellationToken ct = default)
	{
		var tcp = new TcpClient();
		await tcp.ConnectAsync(host, port, ct);

		var ssl = new SslStream(
			tcp.GetStream(),
			leaveInnerStreamOpen: false,
			userCertificateValidationCallback: ValidatePeerCertificate);

		var options = new SslClientAuthenticationOptions
		{
			// Reference Syncthing uses "syncthing" as the SNI name
			TargetHost = "syncthing",
			ClientCertificates = new X509CertificateCollection { _localCert },
			EnabledSslProtocols = SslProtocols.Tls13,
			CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
			RemoteCertificateValidationCallback = ValidatePeerCertificate,
		};

		await ssl.AuthenticateAsClientAsync(options, ct);
		return ssl;
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
}
