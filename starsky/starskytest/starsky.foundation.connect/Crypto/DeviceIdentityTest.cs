using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Crypto;

namespace starskytest.starsky.foundation.connect.Crypto;

[TestClass]
public sealed class DeviceIdentityTest
{
	[TestMethod]
	public void GenerateCertificate_ReturnsCertWithPrivateKey()
	{
		var cert = DeviceIdentity.GenerateCertificate();
		Assert.IsNotNull(cert);
		Assert.IsTrue(cert.HasPrivateKey, "Generated certificate must include a private key.");
	}

	[TestMethod]
	public void GenerateCertificate_ExportedCertCanBeExported()
	{
		// Regression test for macOS Keychain issue where export fails
		var cert = DeviceIdentity.GenerateCertificate();
		
		// Export the generated cert to PKCS12 - this is the real usage pattern
		// We export it once and store as base64, never re-export
		var pfxData = cert.Export(X509ContentType.Pfx);
		Assert.IsNotNull(pfxData);
		Assert.IsTrue(pfxData.Length > 0, "Generated cert should export to PKCS12 successfully.");
		
		cert.Dispose();
	}

	[TestMethod]
	public void DeriveDeviceId_SameCert_ReturnsSameId()
	{
		var cert = DeviceIdentity.GenerateCertificate();
		var id1 = DeviceIdentity.DeriveDeviceId(cert);
		var id2 = DeviceIdentity.DeriveDeviceId(cert);
		Assert.AreEqual(id1, id2);
	}

	[TestMethod]
	public void DeriveDeviceId_HyphenGrouped_HasCorrectStructure()
	{
		var cert = DeviceIdentity.GenerateCertificate();
		var id = DeviceIdentity.DeriveDeviceId(cert);

		// 8 groups of 7 chars separated by 7 hyphens = 8*7 + 7 = 63 chars
		Assert.AreEqual(63, id.Length, $"Device ID should be 63 chars, was: {id}");

		var parts = id.Split('-');
		Assert.AreEqual(8, parts.Length, "Device ID should have 8 groups.");
		foreach ( var part in parts )
		{
			Assert.AreEqual(7, part.Length, $"Each group should be 7 chars, was: '{part}'");
		}
	}

	[TestMethod]
	public void DeriveDeviceIdBytes_Returns32Bytes()
	{
		var cert = DeviceIdentity.GenerateCertificate();
		var bytes = DeviceIdentity.DeriveDeviceIdBytes(cert);
		Assert.AreEqual(32, bytes.Length, "Device ID bytes should be 32 (SHA-256 output).");
	}

	[TestMethod]
	public void NormalizeDeviceId_RemovesHyphens()
	{
		var id = "AAAAAAA-BBBBBBB-CCCCCCC-DDDDDDD-EEEEEEE-FFFFFFF-GGGGGGG-HHHHHHH";
		var normalized = DeviceIdentity.NormalizeDeviceId(id);
		Assert.IsFalse(normalized.Contains('-'), "Normalized ID must not contain hyphens.");
		Assert.AreEqual(56, normalized.Length); // 8*7 = 56 chars without hyphens
	}

	[TestMethod]
	public void AppendLuhnChecksum_ProducesSevenCharGroup()
	{
		var group = DeviceIdentity.AppendLuhnChecksum("ABCDEF");
		Assert.AreEqual(7, group.Length);
		Assert.IsTrue(group.StartsWith("ABCDEF"));
	}

	[TestMethod]
	public void DifferentCerts_ProduceDifferentDeviceIds()
	{
		var cert1 = DeviceIdentity.GenerateCertificate();
		var cert2 = DeviceIdentity.GenerateCertificate();
		var id1 = DeviceIdentity.DeriveDeviceId(cert1);
		var id2 = DeviceIdentity.DeriveDeviceId(cert2);
		Assert.AreNotEqual(id1, id2, "Two different certificates must yield different Device IDs.");
	}
}
