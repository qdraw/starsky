using Starsky.Windows.Models;

namespace Starsky.Windows.Services;

public sealed class FileWorkspaceService
{
    private readonly AppPaths _paths;

    public FileWorkspaceService(AppPaths paths)
    {
        _paths = paths;
    }

    public string GetWorkspaceRoot()
    {
        Directory.CreateDirectory(_paths.TempWorkspacePath);
        return _paths.TempWorkspacePath;
    }

    public string GetParentDirectoryPath(string parentDirectory)
    {
        var normalized = NormalizeRelativePath(parentDirectory);
        var target = string.IsNullOrEmpty(normalized)
            ? _paths.TempWorkspacePath
            : Path.Combine(_paths.TempWorkspacePath, normalized);

        Directory.CreateDirectory(target);
        return target;
    }

    public string GetBinaryTargetPath(DetailViewMetadata detail)
    {
        var lastBinaryPath = detail.CollectionPaths.LastOrDefault(path => !path.EndsWith("xmp", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        var fileName = Path.GetFileName(lastBinaryPath.Replace('/', Path.DirectorySeparatorChar));
        return Path.Combine(GetParentDirectoryPath(detail.ParentDirectory), fileName);
    }

    public string? GetSidecarTargetPath(DetailViewMetadata detail)
    {
        var extension = detail.SidecarExtensionsList.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(extension))
        {
            return null;
        }

        return Path.Combine(
            GetParentDirectoryPath(detail.ParentDirectory),
            $"{detail.FileCollectionName}.{extension}");
    }

    public string BuildSidecarSubPath(DetailViewMetadata detail)
    {
        var extension = detail.SidecarExtensionsList.First();
        var parent = detail.ParentDirectory.TrimEnd('/');
        return $"{parent}/{detail.FileCollectionName}.{extension}";
    }

    private static string NormalizeRelativePath(string input)
    {
        var trimmed = input.Trim().Trim('/');
        return trimmed.Replace('/', Path.DirectorySeparatorChar);
    }
}