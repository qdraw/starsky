using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class FileDownloadService
{
    private readonly HttpClient _http;
    private readonly ILogger<FileDownloadService> _logger;

    public FileDownloadService(ILogger<FileDownloadService> logger)
    {
        _logger = logger;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
    }

    public async Task DownloadAndOpenAsync(string starskyPath, string baseUrl)
    {
        baseUrl = baseUrl.TrimEnd('/');

        // 1. Get file info
        var infoUrl = $"{baseUrl}/starsky/api/index?f={Uri.EscapeDataString(starskyPath)}";
        var infoJson = await _http.GetStringAsync(infoUrl);
        using var doc = JsonDocument.Parse(infoJson);

        // Resolve local path mirroring the server directory structure
        var parentDir = Path.GetDirectoryName(starskyPath)?.TrimStart('/') ?? string.Empty;
        var fileName = Path.GetFileName(starskyPath);
        var localDir = Path.Combine(ApplicationPaths.TempFolder, parentDir.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(localDir);

        var finalPath = Path.Combine(localDir, fileName);
        var tmpPath = finalPath + ".tmp";

        // 2. Download sidecar if present (best-effort)
        try
        {
            var sidecarUrl = $"{baseUrl}/starsky/api/download-sidecar?f={Uri.EscapeDataString(starskyPath)}";
            var sidecarBytes = await _http.GetByteArrayAsync(sidecarUrl);
            if (sidecarBytes.Length > 0)
            {
                var sidecarName = Path.GetFileNameWithoutExtension(fileName) + ".xmp";
                await File.WriteAllBytesAsync(Path.Combine(localDir, sidecarName), sidecarBytes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sidecar download failed or not available for {Path}", starskyPath);
        }

        // 3. Download original file
        _logger.LogInformation("Downloading {Path}", starskyPath);
        var photoUrl = $"{baseUrl}/starsky/api/download-photo?isThumbnail=false&f={Uri.EscapeDataString(starskyPath)}&cache=false";
        var bytes = await _http.GetByteArrayAsync(photoUrl);

        await File.WriteAllBytesAsync(tmpPath, bytes);
        File.Move(tmpPath, finalPath, overwrite: true);
        _logger.LogInformation("Downloaded to {LocalPath}", finalPath);

        // 4. Open with default application
        Process.Start(new ProcessStartInfo(finalPath) { UseShellExecute = true });
    }
}
