using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.Trash;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.Trash;

[TestClass]
public class MacOsTrashBindingHelperTest
{
	[TestMethod]
	public void MacOsTrashBindingHelper_Non_SupportOs()
	{
		var result = MacOsTrashBindingHelper.Trash(new List<string>{"destPath"}, OSPlatform.Linux);
		
		Assert.AreEqual(null, result);
	}
	
	[TestMethod]
	public void MacOsTrashBindingHelper_OnMacOS__MacOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Mac OS Only");
			return;
		}
		
		// the does not need to exist to be true
		var result = MacOsTrashBindingHelper.Trash(new List<string>{"destPath"}, OSPlatform.OSX);
		Assert.AreEqual(true, result);
	}
	
	[TestMethod]
	public void WindowsShellTrashBindingHelperTest_ShouldRemove_OnMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Mac OS Only");
			return;
		}
		
		var createAnImage = new CreateAnImage();
		var destPath = Path.Combine(createAnImage.BasePath, "starsky_unit_test_remove_to_trash.jpg");
		File.Copy(createAnImage.FullFilePath, destPath, true);
			
		var result = MacOsTrashBindingHelper.Trash(destPath, OSPlatform.OSX);

		Assert.AreEqual( true,result);

		var exists = File.Exists(destPath);
		if ( exists )
		{
			File.Delete(destPath);
		}
		Assert.AreEqual(false, exists);	
	}
}
