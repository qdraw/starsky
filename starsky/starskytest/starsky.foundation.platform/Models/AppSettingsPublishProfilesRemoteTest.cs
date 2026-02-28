using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Models.PublishProfileRemote;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsPublishProfilesRemoteTest
{
	[TestMethod]
	public void GetLocalFileSystemById_ReturnsConfiguredCredentials()
	{
		var remote = new AppSettingsPublishProfilesRemote
		{
			Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
			{
				{
					"profile1", [
						new RemoteCredentialWrapper
						{
							Type = RemoteCredentialType.LocalFileSystem,
							LocalFileSystem =
								new LocalFileSystemCredential { Path = "/dest1" }
						}
					]
				}
			}
		};

		var result = remote.GetLocalFileSystemById("profile1");

		Assert.HasCount(1, result);
		Assert.AreEqual("/dest1", result[0].Path);
	}

	[TestMethod]
	public void GetLocalFileSystemById_FallsBackToDefault()
	{
		var remote = new AppSettingsPublishProfilesRemote
		{
			Default =
			[
				new RemoteCredentialWrapper
				{
					Type = RemoteCredentialType.LocalFileSystem,
					LocalFileSystem =
						new LocalFileSystemCredential { Path = "/default-dest" }
				}
			]
		};

		var result = remote.GetLocalFileSystemById("nonexistent");

		Assert.HasCount(1, result);
		Assert.AreEqual("/default-dest", result[0].Path);
	}

	[TestMethod]
	public void GetLocalFileSystemById_ReturnsEmpty_WhenNotConfigured()
	{
		var remote = new AppSettingsPublishProfilesRemote();

		var result = remote.GetLocalFileSystemById("profile1");

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void DisplaySecurity_SetsWarningOnLocalFileSystem()
	{
		var remote = new AppSettingsPublishProfilesRemote
		{
			Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
			{
				{
					"profile1", [
						new RemoteCredentialWrapper
						{
							Type = RemoteCredentialType.LocalFileSystem,
							LocalFileSystem =
								new LocalFileSystemCredential { Path = "/dest" }
						}
					]
				}
			},
			Default =
			[
				new RemoteCredentialWrapper
				{
					Type = RemoteCredentialType.LocalFileSystem,
					LocalFileSystem =
						new LocalFileSystemCredential { Path = "/default" }
				}
			]
		};

		remote.DisplaySecurity("***HIDDEN***");

		var profileCred = remote.Profiles["profile1"][0].LocalFileSystem;
		var defaultCred = remote.Default[0].LocalFileSystem;

		Assert.AreEqual("***HIDDEN***", profileCred?.Path);
		Assert.AreEqual("***HIDDEN***", defaultCred?.Path);
	}

	[TestMethod]
	public void LocalFileSystemCredential_PathGetSet()
	{
		var credential = new LocalFileSystemCredential { Path = "/test/path" };

		Assert.AreEqual("/test/path", credential.Path);
	}

	[TestMethod]
	public void LocalFileSystemCredential_EmptyPath_ReturnsEmpty()
	{
		var credential = new LocalFileSystemCredential();

		Assert.AreEqual(string.Empty, credential.Path);
	}

	[TestMethod]
	public void LocalFileSystemCredential_SetWarning()
	{
		var credential = new LocalFileSystemCredential { Path = "/test" };
		credential.SetWarning("HIDDEN");

		Assert.AreEqual("HIDDEN", credential.Path);
	}

	[TestMethod]
	public void RemoteCredentialType_HasLocalFileSystem()
	{
		var type = RemoteCredentialType.LocalFileSystem;
		Assert.AreEqual("LocalFileSystem", type.ToString());
	}

	// Tests for GetById method with type == null logic
	[TestMethod]
	public void GetById_TypeIsNull_WithProfileFound_ReturnsProfile()
	{
		var ftpWrapper = new RemoteCredentialWrapper
		{
			Type = RemoteCredentialType.Ftp,
			Ftp = new FtpCredential { WebFtp = "ftp://user:pass@example.com/path" }
		};
		var remote = new AppSettingsPublishProfilesRemote
		{
			Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
			{
				{ "profile1", [ftpWrapper] }
			}
		};

		var result = remote.GetById("profile1");

		Assert.HasCount(1, result);
		Assert.AreEqual(RemoteCredentialType.Ftp, result[0].Type);
	}

	[TestMethod]
	public void GetById_TypeIsNull_ProfileNotFound_ReturnDefault()
	{
		var defaultWrapper = new RemoteCredentialWrapper
		{
			Type = RemoteCredentialType.LocalFileSystem,
			LocalFileSystem = new LocalFileSystemCredential { Path = "/default" }
		};
		var remote = new AppSettingsPublishProfilesRemote
		{
			Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
			{
				{
					"profile1", [
						new RemoteCredentialWrapper
						{
							Type = RemoteCredentialType.Ftp,
							Ftp = new FtpCredential { WebFtp = "ftp://user:pass@example.com/path" }
						}
					]
				}
			},
			Default = [defaultWrapper]
		};

		var result = remote.GetById("nonexistent");

		Assert.HasCount(1, result);
		Assert.AreEqual(RemoteCredentialType.LocalFileSystem, result[0].Type);
		Assert.AreEqual("/default", result[0].LocalFileSystem?.Path);
	}

	[TestMethod]
	public void GetById_TypeIsNull_NoProfileAndNoDefault_ReturnsEmpty()
	{
		var remote = new AppSettingsPublishProfilesRemote();

		var result = remote.GetById("nonexistent");

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void GetById_TypeIsNull_MultipleProfilesFound_ReturnsAllProfiles()
	{
		var wrappers = new List<RemoteCredentialWrapper>
		{
			new()
			{
				Type = RemoteCredentialType.Ftp,
				Ftp = new FtpCredential { WebFtp = "ftp://user:pass@example.com/path" }
			},
			new()
			{
				Type = RemoteCredentialType.LocalFileSystem,
				LocalFileSystem = new LocalFileSystemCredential { Path = "/dest" }
			}
		};
		var remote = new AppSettingsPublishProfilesRemote
		{
			Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
			{
				{ "profile1", wrappers }
			}
		};

		var result = remote.GetById("profile1");

		Assert.HasCount(2, result);
		Assert.AreEqual(RemoteCredentialType.Ftp, result[0].Type);
		Assert.AreEqual(RemoteCredentialType.LocalFileSystem, result[1].Type);
	}

	[TestMethod]
	public void GetById_TypeIsNull_EmptyProfileList_ReturnDefault()
	{
		var defaultWrapper = new RemoteCredentialWrapper
		{
			Type = RemoteCredentialType.Ftp,
			Ftp = new FtpCredential { WebFtp = "ftp://user:pass@example.com/path" }
		};
		var remote = new AppSettingsPublishProfilesRemote
		{
			Profiles =
				new Dictionary<string, List<RemoteCredentialWrapper>> { { "profile1", [] } },
			Default = [defaultWrapper]
		};

		var result = remote.GetById("profile1");

		// Empty list is falsy, so result ?? Default returns Default
		Assert.HasCount(1, result);
		Assert.AreEqual(RemoteCredentialType.Ftp, result[0].Type);
	}

	[TestMethod]
	public void ListAll_ReturnsAllCredentialsAndFormatsConsoleOutput()
	{
		var remote = new AppSettingsPublishProfilesRemote
		{
			Default =
			[
				new RemoteCredentialWrapper
				{
					Type = RemoteCredentialType.Ftp,
					Ftp = new FtpCredential { WebFtp = "ftp://dion:dion@default.example.com" }
				},

				new RemoteCredentialWrapper
				{
					Type = RemoteCredentialType.LocalFileSystem,
					LocalFileSystem = new LocalFileSystemCredential { Path = "/default/path" }
				}
			],
			Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
			{
				{
					"profile1", [
						new RemoteCredentialWrapper
						{
							Type = RemoteCredentialType.Ftp,
							Ftp = new FtpCredential
							{
								WebFtp = "ftp://dion:dion@profile1.example.com"
							}
						},

						new RemoteCredentialWrapper
						{
							Type = RemoteCredentialType.LocalFileSystem,
							LocalFileSystem =
								new LocalFileSystemCredential { Path = "/profile1/path" }
						}
					]
				}
			}
		};

		var all = remote.ListAll();
		Assert.HasCount(4, all);
		Assert.AreEqual(( RemoteCredentialType.Ftp, "ftp://dion:dion@default.example.com/" ),
			all[0]);
		Assert.AreEqual(( RemoteCredentialType.Ftp, "ftp://dion:dion@profile1.example.com/" ),
			all[1]);
		Assert.AreEqual(( RemoteCredentialType.LocalFileSystem, "/default/path" ), all[2]);
		Assert.AreEqual(( RemoteCredentialType.LocalFileSystem, "/profile1/path" ), all[3]);

		// Simulate console output
		var outputLines = all.Select(x => $"{x.Item1}: {x.Item2}").ToList();
		Assert.AreEqual("Ftp: ftp://dion:dion@default.example.com/", outputLines[0]);
		Assert.AreEqual("Ftp: ftp://dion:dion@profile1.example.com/", outputLines[1]);
		Assert.AreEqual("LocalFileSystem: /default/path", outputLines[2]);
		Assert.AreEqual("LocalFileSystem: /profile1/path", outputLines[3]);
	}

	[TestMethod]
	public void FtpCredential_WebFtp_ValidUrl_ParsesCorrectly()
	{
		var ftp = new FtpCredential { WebFtp = "ftp://user:pass@ftp.example.com/path" };
		Assert.AreEqual("ftp://user:pass@ftp.example.com/path", ftp.WebFtp);
		Assert.AreEqual("user", ftp.Username);
		Assert.AreEqual("pass", ftp.Password);
		Assert.AreEqual("ftp://ftp.example.com/path", ftp.WebFtpNoLogin);
	}

	[TestMethod]
	public void FtpCredential_WebFtp_Empty_ReturnsEmpty()
	{
		var ftp = new FtpCredential { WebFtp = "" };
		Assert.AreEqual(string.Empty, ftp.WebFtp);
		Assert.AreEqual(string.Empty, ftp.Username);
		Assert.AreEqual(string.Empty, ftp.Password);
		Assert.AreEqual(string.Empty, ftp.WebFtpNoLogin);
	}

	[TestMethod]
	public void FtpCredential_WebFtp_Null_ReturnsEmpty()
	{
		var ftp = new FtpCredential { WebFtp = null! };
		Assert.AreEqual(string.Empty, ftp.WebFtp);
		Assert.AreEqual(string.Empty, ftp.Username);
		Assert.AreEqual(string.Empty, ftp.Password);
		Assert.AreEqual(string.Empty, ftp.WebFtpNoLogin);
	}

	[TestMethod]
	public void FtpCredential_SetWarning_OverridesWebFtp()
	{
		var ftp = new FtpCredential { WebFtp = "ftp://user:pass@ftp.example.com/path" };
		ftp.SetWarning("HIDDEN");
		Assert.Contains("HIDDEN", ftp.WebFtp);
		Assert.AreEqual("ftp://ftp.example.com/path", ftp.WebFtpNoLogin);
	}

	[TestMethod]
	public void FtpCredential_WebFtp_InvalidUrl_DoesNotSet()
	{
		var ftp = new FtpCredential { WebFtp = "not-a-url" };
		Assert.AreEqual(string.Empty, ftp.WebFtp);
		Assert.AreEqual(string.Empty, ftp.WebFtpNoLogin);
	}

	[TestMethod]
	public void FtpCredential_WebFtp_Anonymous_NotSupported()
	{
		var ftp = new FtpCredential { WebFtp = "ftp://anonymous@ftp.example.com/path" };
		Assert.AreEqual(string.Empty, ftp.WebFtp);
	}
}
