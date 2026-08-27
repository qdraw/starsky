using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class FileDownloadService(ILogger<FileDownloadService> logger, HttpClient? http = null)
{
    private readonly HttpClient _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

    [SuppressMessage("Sonar",
	    "S6667: Logging in a catch clause should pass the caught exception as a parameter.",
	    Justification = "Not needed there")]
    public async Task DownloadAndOpenAsync(
        string starskyPath, string baseUrl, bool openFile = true, string? cookieHeader = null)
    {
        baseUrl = baseUrl.TrimEnd('/');

        // 1. Get file info
        var infoUrl = $"{baseUrl}/starsky/api/index?f={Uri.EscapeDataString(starskyPath)}";
        var infoJson = await GetStringAsync(infoUrl, cookieHeader);
        using var doc = JsonDocument.Parse(infoJson);

        // Resolve local path mirroring the server directory structure.
        // TrimStart must strip both / and \ — on Windows GetDirectoryName returns a
        // backslash-prefixed string, and Path.Combine treats a leading \ as drive-relative.
        var parentDir = (Path.GetDirectoryName(starskyPath) ?? string.Empty)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fileName = Path.GetFileName(starskyPath);
        var localDir = Path.Combine(ApplicationPaths.TempFolder, parentDir.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(localDir);

        var finalPath = Path.Combine(localDir, fileName);
        var tmpPath = finalPath + ".tmp";

        // 2. Download sidecar if present (best-effort)
        try
        {
            var sidecarUrl = $"{baseUrl}/starsky/api/download-sidecar?f={Uri.EscapeDataString(starskyPath)}";
            var sidecarBytes = await GetBytesAsync(sidecarUrl, cookieHeader);
            if (sidecarBytes.Length > 0)
            {
                var sidecarName = Path.GetFileNameWithoutExtension(fileName) + ".xmp";
                await File.WriteAllBytesAsync(Path.Combine(localDir, sidecarName), sidecarBytes);
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("No sidecar available for {Path}", starskyPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Sidecar download failed for {Path}", starskyPath);
        }

        // 3. Download original file
        logger.LogInformation("Downloading {Path}", starskyPath);
        var photoUrl = $"{baseUrl}/starsky/api/download-photo?isThumbnail=false&f={Uri.EscapeDataString(starskyPath)}&cache=false";
        var bytes = await GetBytesAsync(photoUrl, cookieHeader);

        await File.WriteAllBytesAsync(tmpPath, bytes);
        File.Move(tmpPath, finalPath, overwrite: true);
        logger.LogInformation("Downloaded to {LocalPath}", finalPath);

        // 4. Open with default application
        if (openFile)
        {
	        OpenWithDefaultApp(finalPath);
        }
    }

    private async Task<string> GetStringAsync(string url, string? cookieHeader)
    {
        if (cookieHeader == null)
        {
	        return await _http.GetStringAsync(url);
        }

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    private async Task<byte[]> GetBytesAsync(string url, string? cookieHeader)
    {
        if (cookieHeader == null)
        {
	        return await _http.GetByteArrayAsync(url);
        }

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync();
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected virtual void OpenWithDefaultApp(string filePath)
        => Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
}
