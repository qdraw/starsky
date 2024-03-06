using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.Trash.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.Trash.Helpers;

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
		Assert.IsTrue(result);
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
		Assert.IsFalse(exists);

		await Task.Delay(500);

		var trashPath =
			Path.Combine(Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile),
				".Trash", fileName);
		
		Assert.IsTrue(File.Exists(trashPath));
		
		File.Delete(trashPath);
	}
	
	[TestMethod]
	public void CreateCfArray__MacOnly()
	{
		if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for Mac OS Only");
			return;
		}

		var result = MacOsTrashBindingHelper.CreateCfArray(new List<IntPtr>().ToArray());
		Assert.IsNotNull(result);
	}
	
	[TestMethod]
	public void CreateCfArrayTest_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for non-Mac OS Only");
			return;
		}

		string? exception = null;
		try
		{
			MacOsTrashBindingHelper.CreateCfArray(new List<IntPtr>().ToArray());
		}
		catch ( DllNotFoundException e )
		{
			exception = e.Message;
		}
		Assert.IsNotNull(exception);
	}
	
	[TestMethod]
	public void CreateCfStringTest_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for non-Mac OS Only");
			return;
		}

		string? exception = null;
		try
		{
			MacOsTrashBindingHelper.CreateCfString("test");
		}
		catch ( DllNotFoundException e )
		{
			exception = e.Message;
		}
		Assert.IsNotNull(exception);
	}

	
	[ExpectedException(typeof(DllNotFoundException))]
	[TestMethod]
	public void TrashInternal_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for non-Mac OS Only");
			return;
		}

		MacOsTrashBindingHelper.TrashInternal(new List<string>());
	}
	
		
	[ExpectedException(typeof(DllNotFoundException))]
	[TestMethod]
	public void GetUrls_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for non-Mac OS Only");
			return;
		}

		MacOsTrashBindingHelper.GetUrls(new List<string>{"value"});
	}
	
	[ExpectedException(typeof(DllNotFoundException))]
	[TestMethod]
	public void CreateCfString_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for non-Mac OS Only");
			return;
		}

		MacOsTrashBindingHelper.CreateCfString("value");
	}
	
	[ExpectedException(typeof(DllNotFoundException))]
	[TestMethod]
	public void CreateCfString_GetSelector()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test if for non-Mac OS Only");
			return;
		}

		MacOsTrashBindingHelper.GetSelector("value");
	}
	
	[TestMethod]
	public void CfStringEncoding_UTF16()
	{
		Assert.AreEqual((uint)0x0100,(uint)MacOsTrashBindingHelper.CfStringEncoding.UTF16);
	}
	
	[TestMethod]
	public void CfStringEncoding_UTF16Be()
	{
		Assert.AreEqual((uint)0x10000100,(uint)MacOsTrashBindingHelper.CfStringEncoding.UTF16BE);
	}
	
	[TestMethod]
	public void CfStringEncoding_UTF16LE()
	{
		Assert.AreEqual((uint)0x14000100,(uint)MacOsTrashBindingHelper.CfStringEncoding.UTF16LE);
	}
	
	[TestMethod]
	public void CfStringEncoding_Ascii()
	{
		Assert.AreEqual((uint)0x0600,(uint)MacOsTrashBindingHelper.CfStringEncoding.ASCII);
	}

}
