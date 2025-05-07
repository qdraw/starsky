using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash.Helpers;
using starsky.foundation.platform.Architecture;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.Trash.Helpers;

[TestClass]
public class MacOsTrashBindingHelperTest
{
	[TestMethod]
	public void MacOsTrashBindingHelper_Non_SupportOs()
	{
		var result =
			MacOsTrashBindingHelper.Trash(new List<string> { "destPath" }, OSPlatform.Linux);

		Assert.IsNull(result);
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
		var result = MacOsTrashBindingHelper.Trash(new List<string> { "destPath" }, OSPlatform.OSX);
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
		var fileName =
			$"starsky_unit_test_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)}.jpg";
		var destPath = Path.Combine(createAnImage.BasePath, fileName);
		File.Copy(createAnImage.FullFilePath, destPath, true);

		var result = MacOsTrashBindingHelper.Trash(destPath, OSPlatform.OSX);

		await Task.Delay(1000);

		Assert.IsTrue(result);

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

		var result = MacOsTrashBindingHelper.CreateCfArray([]);
		Assert.IsNotNull(result.ToString());
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


	[TestMethod]
	public void TrashInternal_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-Mac OS only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsTrashBindingHelper.TrashInternal(new List<string>()));
	}

	[TestMethod]
	public void GetUrls_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-Mac OS only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsTrashBindingHelper.GetUrls(new List<string> { "value" }));
	}

	[TestMethod]
	public void CreateCfString_OtherOs()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-Mac OS only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsTrashBindingHelper.CreateCfString("value"));
	}

	[TestMethod]
	public void CreateCfString_GetSelector()
	{
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.OSX )
		{
			Assert.Inconclusive("This test is for non-Mac OS only");
			return;
		}

		// Act & Assert
		Assert.ThrowsExactly<DllNotFoundException>(() =>
			MacOsTrashBindingHelper.GetSelector("value"));
	}

	[TestMethod]
	[DynamicData(nameof(GetCfStringEncodingTestData), DynamicDataSourceType.Method)]
	public void CfStringEncoding_Tests(uint expected, uint actual)
	{
		Assert.AreEqual(expected, actual);
	}

	public static IEnumerable<object[]> GetCfStringEncodingTestData()
	{
		yield return
		[
			( uint ) 0x0100,
			( uint ) MacOsTrashBindingHelper.CfStringEncoding.UTF16
		];
		yield return
		[
			( uint ) 0x10000100,
			( uint ) MacOsTrashBindingHelper.CfStringEncoding.UTF16BE
		];
		yield return
		[
			( uint ) 0x14000100,
			( uint ) MacOsTrashBindingHelper.CfStringEncoding.UTF16LE
		];
		yield return
		[
			( uint ) 0x0600,
			( uint ) MacOsTrashBindingHelper.CfStringEncoding.ASCII
		];
	}
}
