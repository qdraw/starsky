using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.DataProtection;

namespace starskytest.starsky.foundation.database.DataProtection;

[TestClass]
public sealed class AddDataProtectionKeysCertificateTest
{
	private const string CertificatePassword = "test-password-for-unit-test";

	/// <summary>
	///     Self-signed certificate created for this test only, nothing is shared or reused
	/// </summary>
	private static string CreateTestCertificateFile()
	{
		using var rsa = RSA.Create(2048);
		var request = new CertificateRequest("CN=starsky-unit-test", rsa,
			HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5),
			DateTimeOffset.UtcNow.AddDays(1));

		var path = Path.Combine(Path.GetTempPath(), $"starsky-dp-test-{Guid.NewGuid()}.pfx");
		File.WriteAllBytes(path, certificate.Export(X509ContentType.Pfx, CertificatePassword));
		return path;
	}

	private static (IDataProtector protector, RecordingXmlRepository repository)
		CreateProtector(string? certificatePath, string? password)
	{
		var repository = new RecordingXmlRepository();
		var services = new ServiceCollection();
		services.AddSingleton<IXmlRepository>(repository);
		services.SetupDataProtection(certificatePath, password);
		var provider = services.BuildServiceProvider();
		return (provider.GetRequiredService<IDataProtectionProvider>().CreateProtector("test"),
			repository);
	}

	[TestMethod]
	public void SetupDataProtection_WithCertificate_StoresKeysEncrypted()
	{
		var path = CreateTestCertificateFile();
		try
		{
			var (protector, repository) = CreateProtector(path, CertificatePassword);

			var roundTrip = protector.Unprotect(protector.Protect("secret value"));

			Assert.AreEqual("secret value", roundTrip);
			var stored = string.Join("", repository.Stored.Select(p => p.ToString()));
			Assert.Contains("encryptedSecret", stored);
			Assert.Contains("EncryptedXmlDecryptor", stored);
			Assert.DoesNotContain("masterKey", stored);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void SetupDataProtection_WithoutCertificate_KeepsPreviousBehaviour()
	{
		var (protector, repository) = CreateProtector(null, null);

		protector.Protect("secret value");

		var stored = string.Join("", repository.Stored.Select(p => p.ToString()));
		Assert.Contains("masterKey", stored);
	}

	[TestMethod]
	public void SetupDataProtection_CertificateFileMissing_Throws()
	{
		Assert.ThrowsExactly<FileNotFoundException>(() =>
			CreateProtector(Path.Combine(Path.GetTempPath(), "does-not-exist.pfx"), null));
	}

	[TestMethod]
	public void SetupDataProtection_WrongPassword_ThrowsInsteadOfFallingBackToPlainKeys()
	{
		var path = CreateTestCertificateFile();
		try
		{
			Assert.ThrowsExactly<InvalidOperationException>(() =>
				CreateProtector(path, "wrong-password"));
		}
		finally
		{
			File.Delete(path);
		}
	}

	private sealed class RecordingXmlRepository : IXmlRepository
	{
		public List<XElement> Stored { get; } = [];

		public IReadOnlyCollection<XElement> GetAllElements()
		{
			return Stored;
		}

		public void StoreElement(XElement element, string friendlyName)
		{
			Stored.Add(element);
		}
	}
}
