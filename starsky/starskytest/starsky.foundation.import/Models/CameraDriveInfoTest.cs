using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Models;

namespace starskytest.starsky.foundation.import.Models;

[TestClass]
public class CameraDriveInfoTest
{
    [TestMethod]
    public void ToCameraDriveInfo_NullDriveInfo_ReturnsDefaults()
    {
        DriveInfo? drive = null;
        var result = drive.ToCameraDriveInfo();
        Assert.IsFalse(result.IsReady);
        Assert.IsFalse(result.RootDirectory.Exists);
        Assert.AreEqual(string.Empty, result.RootDirectory.FullName);
        Assert.AreEqual(string.Empty, result.DriveFormat);
    }

    [TestMethod]
    public void ToCameraDriveInfo_ValidDriveInfo_MapsProperties()
    {
        // Use a real drive for test, but only check that mapping is consistent
        var drive = DriveInfo.GetDrives()[0];
        var result = drive.ToCameraDriveInfo();
        Assert.AreEqual(drive.IsReady, result.IsReady);
        Assert.AreEqual(drive.RootDirectory.Exists, result.RootDirectory.Exists);
        Assert.AreEqual(drive.RootDirectory.FullName, result.RootDirectory.FullName);
        Assert.AreEqual(drive.DriveFormat, result.DriveFormat);
    }
}

