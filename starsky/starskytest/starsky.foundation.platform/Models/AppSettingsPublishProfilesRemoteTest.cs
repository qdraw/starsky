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
					"profile1",
					new List<RemoteCredentialWrapper>
					{
						new()
						{
							Type = RemoteCredentialType.LocalFileSystem,
							LocalFileSystem =
								new LocalFileSystemCredential { Path = "/dest1" }
						}
					}
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
			Default = new List<RemoteCredentialWrapper>
			{
				new()
				{
					Type = RemoteCredentialType.LocalFileSystem,
					LocalFileSystem =
						new LocalFileSystemCredential { Path = "/default-dest" }
				}
			}
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
					"profile1",
					new List<RemoteCredentialWrapper>
					{
						new()
						{
							Type = RemoteCredentialType.LocalFileSystem,
							LocalFileSystem =
								new LocalFileSystemCredential { Path = "/dest" }
						}
					}
				}
			},
			Default = new List<RemoteCredentialWrapper>
			{
				new()
				{
					Type = RemoteCredentialType.LocalFileSystem,
					LocalFileSystem =
						new LocalFileSystemCredential { Path = "/default" }
				}
			}
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
}
