using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

/// <summary>
/// Only for non Windows - so no tests - This feature is windows specific
/// </summary>
[TestClass]
public class WindowsSetFileAssociationsUnixTests
{
	[TestMethod]
	public void EnsureAssociationsSet__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Mac and Linux Only");
			return;
		}

		var result = WindowsSetFileAssociations.EnsureAssociationsSet(new FileAssociation
		{
			Extension = ".jpg",
			ProgId = "starsky",
			FileTypeDescription = "Starsky Test File",
			ExecutableFilePath = "/usr/bin/starsky"
		});

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void SetKeyDefaultValue__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Mac and Linux Only");
			return;
		}

		// Is false due its unix
		var result = WindowsSetFileAssociations.SetKeyDefaultValue("test", "Test");
		Assert.IsFalse(result);
	}
}
