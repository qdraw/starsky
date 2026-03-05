using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Models.PublishProfileRemote;

namespace starskytest.starsky.foundation.platform.Models.PublishProfileRemote;

[TestClass]
public class FtpCredentialTest
{
	[TestMethod]
	[DataRow("/upload/images", "/upload/images")]
	[DataRow("upload/images", "/upload/images")]
	[DataRow("", "/")]
	[DataRow("   ", "/")]
	public void Path_SetValue_Normalizes(string input, string expected)
	{
		var credential = new FtpCredential { Path = input };

		Assert.AreEqual(expected, credential.Path);
	}

	[TestMethod]
	public void Path_SetNull_DefaultsToSlash()
	{
		var credential = new FtpCredential { Path = null! };

		Assert.AreEqual("/", credential.Path);
	}

	[TestMethod]
	public void WebFtp_AllFieldsPopulated_ReturnsFullUrl()
	{
		var credential = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "user123",
			Password = "pass456",
			Path = "/upload"
		};

		var result = credential.WebFtp;

		Assert.AreEqual("ftp://user123:pass456@ftp.example.com/upload", result);
	}

	[TestMethod]
	public void WebFtp_UsernameWithSpecialChars_PercentEncodes()
	{
		var credential = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "user@domain.com",
			Password = "pass123",
			Path = "/upload"
		};

		var result = credential.WebFtp;

		Assert.AreEqual("ftp://user%40domain.com:pass123@ftp.example.com/upload", result);
	}

	[TestMethod]
	public void WebFtp_PasswordWithSpecialChars_PercentEncodes()
	{
		var credential = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "user123",
			Password = "p@ss:word!",
			Path = "/upload"
		};

		var result = credential.WebFtp;

		Assert.AreEqual("ftp://user123:p%40ss%3Aword%21@ftp.example.com/upload", result);
	}

	[TestMethod]
	public void WebFtp_SecurityWarningPassword_DoesNotEncode()
	{
		var credential = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "user123",
			Password = AppSettings.CloneToDisplaySecurityWarning,
			Path = "/upload"
		};

		var result = credential.WebFtp;

		Assert.AreEqual(
			$"ftp://user123:{AppSettings.CloneToDisplaySecurityWarning}@ftp.example.com/upload",
			result);
	}

	[TestMethod]
	[DataRow("", "user123", "pass456")]
	[DataRow("ftp.example.com", "", "pass456")]
	[DataRow("ftp.example.com", "user123", "")]
	[DataRow(null, "user123", "pass456")]
	[DataRow("ftp.example.com", null, "pass456")]
	[DataRow("ftp.example.com", "user123", null)]
	public void WebFtp_MissingParts_ReturnsEmptyString(string? host,
		string? username, string? password)
	{
		var credential = new FtpCredential
		{
			Host = host!, Username = username!, Password = password!, Path = "/upload"
		};

		Assert.AreEqual(string.Empty, credential.WebFtp);
	}

	[TestMethod]
	public void WebFtp_SetValidUrl_ParsesCorrectly()
	{
		var credential = new FtpCredential
		{
			WebFtp = "ftp://user123:pass456@ftp.example.com/upload/images"
		};

		Assert.AreEqual("ftp.example.com", credential.Host);
		Assert.AreEqual("user123", credential.Username);
		Assert.AreEqual("pass456", credential.Password);
		Assert.AreEqual("/upload/images", credential.Path);
		Assert.AreEqual("ftp", credential.Scheme);
	}

	[TestMethod]
	public void WebFtp_SetUrlWithEncodedUsername_Unescapes()
	{
		var credential = new FtpCredential
		{
			WebFtp = "ftp://user%40domain.com:pass456@ftp.example.com/upload"
		};

		Assert.AreEqual("user@domain.com", credential.Username);
		Assert.AreEqual("pass456", credential.Password);
	}

	[TestMethod]
	public void WebFtp_SetUrlWithEncodedPassword_Unescapes()
	{
		var credential = new FtpCredential
		{
			WebFtp = "ftp://user123:p%40ss%3Aword%21@ftp.example.com/upload"
		};

		Assert.AreEqual("user123", credential.Username);
		Assert.AreEqual("p@ss:word!", credential.Password);
	}


	[TestMethod]
	[DataRow("not a valid uri")]
	[DataRow("/relative/path")]
	[DataRow("http://user123:pass456@example.com/path")]
	[DataRow("https://user123:pass456@example.com/path")]
	[DataRow("ftp://ftp.example.com/upload")]
	[DataRow("ftp://user123@ftp.example.com/upload")]
	public void WebFtp_SetInvalidOrUnsupportedUrl_DoesNotModifyProperties(string url)
	{
		var credential = new FtpCredential
		{
			Host = "original.com", Username = "original", Password = "original", WebFtp = url
		};

		Assert.AreEqual("original.com", credential.Host);
		Assert.AreEqual("original", credential.Username);
		Assert.AreEqual("original", credential.Password);
	}

	[TestMethod]
	public void WebFtp_SetFtpsUrl_ParsesCorrectly()
	{
		var credential = new FtpCredential
		{
			WebFtp = "ftps://user123:pass456@ftp.example.com/upload"
		};

		Assert.AreEqual("ftp.example.com", credential.Host);
		Assert.AreEqual("user123", credential.Username);
		Assert.AreEqual("pass456", credential.Password);
		Assert.AreEqual("ftps", credential.Scheme);
	}

	[TestMethod]
	public void SetWarning_SetsPasswordToWarning()
	{
		var credential = new FtpCredential { Password = "original" };

		credential.SetWarning("SECURITY WARNING");

		Assert.AreEqual("SECURITY WARNING", credential.Password);
	}

	[TestMethod]
	public void Scheme_DefaultValue_IsFtp()
	{
		var credential = new FtpCredential();

		Assert.AreEqual("ftp", credential.Scheme);
	}

	[TestMethod]
	public void Scheme_CanBeSet()
	{
		var credential = new FtpCredential { Scheme = "ftps" };

		Assert.AreEqual("ftps", credential.Scheme);
	}

	[TestMethod]
	public void WebFtp_ComplexPath_EncodesCorrectly()
	{
		var credential = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "my user",
			Password = "my pass",
			Path = "/folder/subfolder"
		};

		var result = credential.WebFtp;

		Assert.AreEqual("ftp://my%20user:my%20pass@ftp.example.com/folder/subfolder", result);
	}

	[TestMethod]
	public void WebFtp_SetUrlWithComplexPath_ParsesCorrectly()
	{
		var credential = new FtpCredential
		{
			WebFtp = "ftp://user123:pass456@ftp.example.com/level1/level2/level3"
		};

		Assert.AreEqual("/level1/level2/level3", credential.Path);
	}

	[TestMethod]
	public void WebFtp_SetUrlWithSpacesInCredentials_Unescapes()
	{
		var credential = new FtpCredential
		{
			WebFtp = "ftp://my%20user:my%20pass@ftp.example.com/upload"
		};

		Assert.AreEqual("my user", credential.Username);
		Assert.AreEqual("my pass", credential.Password);
	}

	[TestMethod]
	public void WebFtp_RoundTrip_PreservesData()
	{
		var original = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "user123",
			Password = "pass456",
			Path = "/upload",
			Scheme = "ftps"
		};

		var url = original.WebFtp;
		var restored = new FtpCredential { WebFtp = url };

		Assert.AreEqual(original.Host, restored.Host);
		Assert.AreEqual(original.Username, restored.Username);
		Assert.AreEqual(original.Password, restored.Password);
		Assert.AreEqual(original.Path, restored.Path);
		Assert.AreEqual(original.Scheme, restored.Scheme);
	}

	[TestMethod]
	public void WebFtp_RoundTripWithSpecialChars_PreservesData()
	{
		var original = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "user@domain.com",
			Password = "p@ss:word!",
			Path = "/upload/images"
		};

		var url = original.WebFtp;
		var restored = new FtpCredential { WebFtp = url };

		Assert.AreEqual(original.Host, restored.Host);
		Assert.AreEqual(original.Username, restored.Username);
		Assert.AreEqual(original.Password, restored.Password);
		Assert.AreEqual(original.Path, restored.Path);
	}

	[TestMethod]
	public void Host_CanBeSetAndRetrieved()
	{
		var credential = new FtpCredential { Host = "ftp.example.com" };

		Assert.AreEqual("ftp.example.com", credential.Host);
	}

	[TestMethod]
	public void Username_CanBeSetAndRetrieved()
	{
		var credential = new FtpCredential { Username = "myuser" };

		Assert.AreEqual("myuser", credential.Username);
	}

	[TestMethod]
	public void Password_CanBeSetAndRetrieved()
	{
		var credential = new FtpCredential { Password = "mypassword" };

		Assert.AreEqual("mypassword", credential.Password);
	}

	[TestMethod]
	public void DefaultProperties_AreEmpty()
	{
		var credential = new FtpCredential();

		Assert.AreEqual(string.Empty, credential.Host);
		Assert.AreEqual(string.Empty, credential.Username);
		Assert.AreEqual(string.Empty, credential.Password);
	}

	[TestMethod]
	public void Path_MultipleSlashes_NormalizedCorrectly()
	{
		var credential = new FtpCredential { Path = "///upload/images" };

		Assert.AreEqual("///upload/images", credential.Path);
	}

	[TestMethod]
	public void WebFtp_SetUrlWithPort_ParsesHostWithPort()
	{
		var credential = new FtpCredential
		{
			WebFtp = "ftp://user123:pass456@ftp.example.com:2121/upload"
		};

		Assert.AreEqual("ftp.example.com", credential.Host);
		Assert.AreEqual("user123", credential.Username);
	}

	[TestMethod]
	public void WebFtp_SetEmptyPassword_ParsesWithEmptyPassword()
	{
		var credential = new FtpCredential
		{
			Host = "original.com", Username = "original", Password = "original", WebFtp = "ftp://useronly:@ftp.example.com/upload"
		};

		// Should parse - empty password is valid (parts.Length == 2)
		Assert.AreEqual("ftp.example.com", credential.Host);
		Assert.AreEqual("useronly", credential.Username);
		Assert.AreEqual("", credential.Password);
	}

	[TestMethod]
	public void Path_SetPathWithBackslash_PreservesBackslash()
	{
		var credential = new FtpCredential { Path = "\\upload\\images" };

		Assert.AreEqual(@"/\upload\images", credential.Path);
	}

	[TestMethod]
	public void WebFtp_WithDifferentScheme_UsesConfiguredScheme()
	{
		var credential = new FtpCredential
		{
			Host = "ftp.example.com",
			Username = "user123",
			Password = "pass456",
			Path = "/upload",
			Scheme = "ftps"
		};

		var result = credential.WebFtp;

		Assert.AreEqual("ftps://user123:pass456@ftp.example.com/upload", result);
	}

	[TestMethod]
	public void WebFtp_SetUrlUpdatesScheme()
	{
		var credential = new FtpCredential
		{
			Scheme = "custom", WebFtp = "ftp://user123:pass456@ftp.example.com/upload" // Start with non-default
		};

		Assert.AreEqual("ftp", credential.Scheme);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void WebFtp_SetBlankUrl_DoesNotModifyProperties(string? url)
	{
		var credential = new FtpCredential
		{
			Host = "original.com",
			Username = "original",
			Password = "original",
			Path = "/original",
			WebFtp = url!
		};

		Assert.AreEqual("original.com", credential.Host);
		Assert.AreEqual("original", credential.Username);
		Assert.AreEqual("original", credential.Password);
		Assert.AreEqual("/original", credential.Path);
	}
}
