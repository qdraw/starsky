namespace Starsky.Windows.Models;

public sealed class DetailViewMetadata
{
    public string ParentDirectory { get; init; } = string.Empty;

    public string FileCollectionName { get; init; } = string.Empty;

    public IReadOnlyList<string> CollectionPaths { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> SidecarExtensionsList { get; init; } = Array.Empty<string>();
}