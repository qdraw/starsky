using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.database.DataProtection;

public static class AddDataProtectionKeys
{
	/// <summary>
	///     Store the data protection keys in the database
	/// </summary>
	/// <param name="services">DI</param>
	/// <param name="certificatePath">
	///     optional PKCS#12 file, when set the keys are encrypted with it at rest
	/// </param>
	/// <param name="certificatePassword">optional password of the certificate</param>
	public static void SetupDataProtection(this IServiceCollection services,
		string? certificatePath = null, string? certificatePassword = null)
	{
		var builder = services.AddDataProtection()
			.AddKeyManagementOptions(options => options.XmlRepository =
				services.BuildServiceProvider().GetService<IXmlRepository>())
			.SetApplicationName("Starsky");
		// Application Name is using by default the path where it runs from
		// and SetApplicationName does overwrite it

		if ( string.IsNullOrWhiteSpace(certificatePath) )
		{
			return;
		}

		builder.ProtectKeysWithCertificate(LoadCertificate(certificatePath, certificatePassword));
	}

	/// <summary>
	///     Fail on startup when the configured certificate can not be used: silently falling back
	///     to unencrypted keys would hide a security setting that does not work
	/// </summary>
	internal static X509Certificate2 LoadCertificate(string certificatePath,
		string? certificatePassword)
	{
		if ( !File.Exists(certificatePath) )
		{
			throw new FileNotFoundException(
				"DataProtectionCertificatePath is set but the file does not exist",
				certificatePath);
		}

		try
		{
			return X509CertificateLoader.LoadPkcs12FromFile(certificatePath,
				string.IsNullOrEmpty(certificatePassword) ? null : certificatePassword);
		}
		catch ( CryptographicException exception )
		{
			throw new InvalidOperationException(
				"DataProtectionCertificatePath could not be loaded as PKCS#12, " +
				"check the file and DataProtectionCertificatePassword", exception);
		}
	}
}
