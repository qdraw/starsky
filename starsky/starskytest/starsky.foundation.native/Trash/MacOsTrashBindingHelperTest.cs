using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
	public async Task Trash_ShouldRemove_OnMacOS()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Mac OS Only");
			return;
		}
		
		var createAnImage = new CreateAnImage();
		var fileName = $"starsky_unit_test_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)}.jpg";
		var destPath = Path.Combine(createAnImage.BasePath, fileName);
		File.Copy(createAnImage.FullFilePath, destPath, true);
			
		var result = MacOsTrashBindingHelper.Trash(destPath, OSPlatform.OSX);
		
		await Task.Delay(1000);

		Assert.AreEqual( true,result);

		var exists = File.Exists(destPath);
		if ( exists )
		{
			File.Delete(destPath);
		}
		Assert.AreEqual(false, exists);

		await Task.Delay(500);

		var trashPath =
			Path.Combine(Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile),
				".Trash", fileName);
		
		Assert.IsTrue(File.Exists(trashPath));
		
		File.Delete(trashPath);
	}
}
