using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace starsky.foundation.connect.Crypto;

/// <summary>
/// Generates self-signed TLS certificates and derives BEP v1 Device IDs from them.
/// </summary>
public static class DeviceIdentity
{
	/// <summary>
	/// Generates a self-signed RSA-2048 certificate valid for 20 years,
	/// suitable for use as a BEP v1 device identity.
	/// </summary>
	public static X509Certificate2 GenerateCertificate()
	{
		using var key = RSA.Create(2048);

		var request = new CertificateRequest(
			new X500DistinguishedName("CN=syncthing"),
			key,
			HashAlgorithmName.SHA256,
			RSASignaturePadding.Pkcs1);

		request.CertificateExtensions.Add(
			new X509KeyUsageExtension(
				X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
				critical: false));

		request.CertificateExtensions.Add(
			new X509EnhancedKeyUsageExtension(
				[
					new Oid("1.3.6.1.5.5.7.3.1"), // TLS server
					new Oid("1.3.6.1.5.5.7.3.2"), // TLS client
				],
				critical: false));

		var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
		var notAfter = notBefore.AddYears(20);

		return request.CreateSelfSigned(notBefore, notAfter);
	}

	/// <summary>
	/// Derives the BEP Device ID from a certificate:
	/// Base32(SHA-256(cert.RawData)) grouped into 8×7 chars with hyphens.
	/// Each group has a Luhn-mod-N checksum character appended.
	/// </summary>
	public static string DeriveDeviceId(X509Certificate2 cert)
	{
		var hash = SHA256.HashData(cert.RawData);
		var b32 = Base32.Encode(hash); // 52 chars for 32 bytes

		// Luhnify: four 13-char chunks, each gets a Luhn checksum → 4 × 14 = 56 chars.
		// Then chunkify into eight 7-char groups separated by hyphens.
		var luhnified = string.Concat(Enumerable.Range(0, 4)
			.Select(i => AppendLuhnChecksum(b32.Substring(i * 13, 13))));

		var groups = Enumerable.Range(0, 8)
			.Select(i => luhnified.Substring(i * 7, 7));

		return string.Join('-', groups);
	}

	/// <summary>
	/// Returns the raw 32-byte device ID (SHA-256 of cert raw bytes).
	/// This is what BEP transmits on the wire in Device.id fields.
	/// </summary>
	public static byte[] DeriveDeviceIdBytes(X509Certificate2 cert)
	{
		return SHA256.HashData(cert.RawData);
	}

	/// <summary>
	/// Computes and appends a Luhn-mod-32 checksum character to a base32 string.
	/// Matches the reference Syncthing implementation.
	/// </summary>
	public static string AppendLuhnChecksum(string group)
	{
		const int n = 32;
		var factor = 1;
		var sum = 0;

		for ( var i = group.Length - 1; i >= 0; i-- )
		{
			var codePoint = Base32.Alphabet.IndexOf(group[i]);
			var addend = factor * codePoint;
			factor = factor == 2 ? 1 : 2;
			addend = ( addend / n ) + ( addend % n );
			sum += addend;
		}

		var remainder = sum % n;
		var checkCodePoint = ( n - remainder ) % n;
		return group + Base32.Alphabet[checkCodePoint];
	}

	/// <summary>
	/// Parses a human-supplied device ID to the canonical base32 form (no hyphens, uppercase).
	/// Applies Syncthing typo corrections before validation: 0→O, 1→I, 8→B.
	/// </summary>
	public static string NormalizeDeviceId(string deviceId)
	{
		return deviceId
			.Replace("-", string.Empty, StringComparison.Ordinal)
			.ToUpperInvariant()
			.Replace('0', 'O')
			.Replace('1', 'I')
			.Replace('8', 'B');
	}
}
