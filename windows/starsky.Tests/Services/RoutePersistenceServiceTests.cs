using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public sealed class RoutePersistenceServiceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly SettingsService _settings;
    private readonly RoutePersistenceService _sut;

    public RoutePersistenceServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"starsky-routes-{Guid.NewGuid()}.json");
        _settings = new SettingsService(NullLogger<SettingsService>.Instance, _tempFile);
        _settings.Load();
        _sut = new RoutePersistenceService(_settings);
    }

    [Fact]
    public void GetRoutes_WhenEmpty_ReturnsEmptyList()
    {
        Assert.Empty(_sut.GetRoutes());
    }

    [Fact]
    public void SaveRoute_AddsEntry()
    {
        _sut.SaveRoute(0, "?f=/photos");

        var routes = _sut.GetRoutes();
        Assert.Single(routes);
        Assert.Equal("?f=/photos", routes[0].Route);
    }

    [Fact]
    public void SaveRoute_WithGeometry_PersistsGeometry()
    {
        var geo = new SavedWindowState { Left = 50, Top = 60, Width = 800, Height = 600, IsMaximized = true };

        _sut.SaveRoute(0, "?f=/", geo);

        var state = _sut.GetRoutes()[0];
        Assert.Equal(50, state.Left);
        Assert.Equal(60, state.Top);
        Assert.Equal(800, state.Width);
        Assert.Equal(600, state.Height);
        Assert.True(state.IsMaximized);
    }

    [Fact]
    public void SaveRoute_ExpandsListWithBlanks()
    {
        _sut.SaveRoute(2, "?f=/deep");

        var routes = _sut.GetRoutes();
        Assert.Equal(3, routes.Count);
        Assert.Equal("?f=/", routes[0].Route);
        Assert.Equal("?f=/", routes[1].Route);
        Assert.Equal("?f=/deep", routes[2].Route);
    }

    [Fact]
    public void RemoveRoute_RemovesEntry()
    {
        _sut.SaveRoute(0, "?f=/a");
        _sut.SaveRoute(1, "?f=/b");

        _sut.RemoveRoute(0);

        var routes = _sut.GetRoutes();
        Assert.Single(routes);
        Assert.Equal("?f=/b", routes[0].Route);
    }

    [Fact]
    public void ClearAll_EmptiesList()
    {
        _sut.SaveRoute(0, "?f=/a");
        _sut.SaveRoute(1, "?f=/b");

        _sut.ClearAll();

        Assert.Empty(_sut.GetRoutes());
    }

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
