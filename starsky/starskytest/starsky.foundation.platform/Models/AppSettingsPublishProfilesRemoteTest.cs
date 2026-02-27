using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

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

		var result = remote.GetById("profile1", type: null);

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
							Ftp = new FtpCredential
							{
								WebFtp = "ftp://user:pass@example.com/path"
							}
						}
					]
				}
			},
			Default = [defaultWrapper]
		};

		var result = remote.GetById("nonexistent", type: null);

		Assert.HasCount(1, result);
		Assert.AreEqual(RemoteCredentialType.LocalFileSystem, result[0].Type);
		Assert.AreEqual("/default", result[0].LocalFileSystem?.Path);
	}

	[TestMethod]
	public void GetById_TypeIsNull_NoProfileAndNoDefault_ReturnsEmpty()
	{
		var remote = new AppSettingsPublishProfilesRemote();

		var result = remote.GetById("nonexistent", type: null);

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

		var result = remote.GetById("profile1", type: null);

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
			Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
			{
				{ "profile1", [] }
			},
			Default = [defaultWrapper]
		};

		var result = remote.GetById("profile1", type: null);

		// Empty list is falsy, so result ?? Default returns Default
		Assert.HasCount(1, result);
		Assert.AreEqual(RemoteCredentialType.Ftp, result[0].Type);
	}
}
