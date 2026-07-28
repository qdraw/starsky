using System.Text.Json;
using Starsky.Windows.Models;

namespace Starsky.Windows.Services;

public sealed class BackendApiClient
{
    private readonly HttpClient _httpClient;

    public BackendApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UrlValidationResult> ValidateRemoteUrlAsync(string location, CancellationToken cancellationToken)
    {
        var trimmedLocation = location.Trim().TrimEnd('/');
        if (!Uri.TryCreate(trimmedLocation, UriKind.Absolute, out var uri))
        {
            return new UrlValidationResult { IsValid = false, IsLocal = false, Location = location, Reason = "Invalid URL" };
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            return new UrlValidationResult { IsValid = false, IsLocal = false, Location = location, Reason = "Only HTTP and HTTPS are supported" };
        }

        try
        {
            var response = await _httpClient.GetAsync(new Uri(uri, "/api/health"), cancellationToken);
            var isValid = response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.ServiceUnavailable;

            return new UrlValidationResult
            {
                IsValid = isValid,
                IsLocal = false,
                Location = trimmedLocation,
                Reason = isValid ? null : $"Health endpoint returned {(int)response.StatusCode}",
            };
        }
        catch (Exception exception)
        {
            return new UrlValidationResult
            {
                IsValid = false,
                IsLocal = false,
                Location = location,
                Reason = exception.Message,
            };
        }
    }

    public async Task<DetailViewMetadata?> GetDetailViewAsync(Uri baseUri, string filePath, CancellationToken cancellationToken)
    {
        var uri = new Uri(baseUri, $"/starsky/api/index?f={Uri.EscapeDataString(filePath)}");
        await using var stream = await _httpClient.GetStreamAsync(uri, cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("fileIndexItem", out var fileIndexItem))
        {
            return null;
        }

        var collectionPaths = fileIndexItem.TryGetProperty("collectionPaths", out var collectionElement)
            ? collectionElement.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToList()
            : new List<string>();

        var sidecarExtensions = fileIndexItem.TryGetProperty("sidecarExtensionsList", out var sidecarElement)
            ? sidecarElement.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToList()
            : new List<string>();

        return new DetailViewMetadata
        {
            ParentDirectory = fileIndexItem.GetProperty("parentDirectory").GetString() ?? string.Empty,
            FileCollectionName = fileIndexItem.GetProperty("fileCollectionName").GetString() ?? string.Empty,
            CollectionPaths = collectionPaths,
            SidecarExtensionsList = sidecarExtensions,
        };
    }

    public async Task DownloadToFileAsync(Uri downloadUri, string filePath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await using var source = await _httpClient.GetStreamAsync(downloadUri, cancellationToken);
        await using var target = File.Create(filePath);
        await source.CopyToAsync(target, cancellationToken);
    }
}