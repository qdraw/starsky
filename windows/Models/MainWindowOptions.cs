using Microsoft.Extensions.Logging;
using Starsky.Desktop.Services;

namespace Starsky.Desktop.Models;

public sealed record MainWindowOptions
{
    public required SettingsService Settings { get; init; }
    public required RoutePersistenceService Routes { get; init; }
    public required WebViewEnvironmentService WebViewEnv { get; init; }
    public required FileDownloadService FileDownload { get; init; }
    public required WindowManager WindowManager { get; init; }
    public required ILogger Logger { get; init; }
    public required string BaseUrl { get; init; }
    public required string InitialRoute { get; init; }
    public required SavedWindowState Geometry { get; init; }
    public required int WindowIndex { get; init; }
}
