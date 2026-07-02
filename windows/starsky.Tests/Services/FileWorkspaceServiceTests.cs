using Microsoft.VisualStudio.TestTools.UnitTesting;
using Starsky.Windows.Models;
using Starsky.Windows.Services;

namespace Starsky.Tests.Services;

[TestClass]
public class FileWorkspaceServiceTests
{
    [TestMethod]
    public void GetWorkspaceRoot_ReturnsConfiguredPath_AndCreatesDirectory()
    {
        var service = new FileWorkspaceService(new AppPaths());

        var root = service.GetWorkspaceRoot();

        Assert.IsFalse(string.IsNullOrWhiteSpace(root));
        Assert.IsTrue(Directory.Exists(root));
    }

    [TestMethod]
    public void GetParentDirectoryPath_NormalizesSlashesAndWhitespace()
    {
        var service = new FileWorkspaceService(new AppPaths());

        var target = service.GetParentDirectoryPath(" /root/child/ ");

        Assert.IsTrue(Directory.Exists(target));
        StringAssert.EndsWith(target, Path.Combine("root", "child"));
    }

    [TestMethod]
    public void GetParentDirectoryPath_ReturnsWorkspace_WhenInputNormalizesToEmpty()
    {
        var service = new FileWorkspaceService(new AppPaths());

        var target = service.GetParentDirectoryPath("///");
        var workspace = service.GetWorkspaceRoot();

        Assert.AreEqual(workspace, target);
    }

    [TestMethod]
    public void GetBinaryTargetPath_UsesLastNonXmpCollectionPath()
    {
        var service = new FileWorkspaceService(new AppPaths());
        var detail = new DetailViewMetadata
        {
            ParentDirectory = "albums/2026",
            CollectionPaths = new[] { "albums/2026/photo.xmp", "albums/2026/photo.jpg" },
        };

        var result = service.GetBinaryTargetPath(detail);

        StringAssert.EndsWith(result, Path.Combine("albums", "2026", "photo.jpg"));
    }

    [TestMethod]
    public void GetBinaryTargetPath_ReturnsParentPath_WhenNoCollectionPathExists()
    {
        var service = new FileWorkspaceService(new AppPaths());
        var detail = new DetailViewMetadata
        {
            ParentDirectory = "albums/2026",
            CollectionPaths = Array.Empty<string>(),
        };

        var result = service.GetBinaryTargetPath(detail);

        StringAssert.EndsWith(result, Path.Combine("albums", "2026"));
    }

    [TestMethod]
    public void GetSidecarTargetPath_ReturnsNull_WhenNoExtension()
    {
        var service = new FileWorkspaceService(new AppPaths());
        var detail = new DetailViewMetadata
        {
            ParentDirectory = "albums/2026",
            FileCollectionName = "photo",
            SidecarExtensionsList = new[] { "" },
        };

        var result = service.GetSidecarTargetPath(detail);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetSidecarTargetPath_ReturnsTargetPath_WhenExtensionIsPresent()
    {
        var service = new FileWorkspaceService(new AppPaths());
        var detail = new DetailViewMetadata
        {
            ParentDirectory = "albums/2026",
            FileCollectionName = "photo",
            SidecarExtensionsList = new[] { "xmp" },
        };

        var result = service.GetSidecarTargetPath(detail);

        Assert.IsNotNull(result);
        StringAssert.EndsWith(result, Path.Combine("albums", "2026", "photo.xmp"));
    }

    [TestMethod]
    public void BuildSidecarSubPath_CombinesParentNameAndExtension()
    {
        var service = new FileWorkspaceService(new AppPaths());
        var detail = new DetailViewMetadata
        {
            ParentDirectory = "albums/2026/",
            FileCollectionName = "photo",
            SidecarExtensionsList = new[] { "xmp" },
        };

        var result = service.BuildSidecarSubPath(detail);

        Assert.AreEqual("albums/2026/photo.xmp", result);
    }
}

