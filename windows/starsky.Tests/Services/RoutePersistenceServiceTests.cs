using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

[TestClass]
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

    [TestMethod]
    public void GetRoutes_WhenEmpty_ReturnsEmptyList()
    {
        Assert.AreEqual(0, _sut.GetRoutes().Count);
    }

    [TestMethod]
    public void SaveRoute_AddsEntry()
    {
        _sut.SaveRoute(0, "?f=/photos");

        var routes = _sut.GetRoutes();
        Assert.AreEqual(1, routes.Count);
        Assert.AreEqual("?f=/photos", routes[0].Route);
    }

    [TestMethod]
    public void SaveRoute_WithGeometry_PersistsGeometry()
    {
        var geo = new SavedWindowState { Left = 50, Top = 60, Width = 800, Height = 600, IsMaximized = true };

        _sut.SaveRoute(0, "?f=/", geo);

        var state = _sut.GetRoutes()[0];
        Assert.AreEqual(50, state.Left);
        Assert.AreEqual(60, state.Top);
        Assert.AreEqual(800, state.Width);
        Assert.AreEqual(600, state.Height);
        Assert.IsTrue(state.IsMaximized);
    }

    [TestMethod]
    public void SaveRoute_ExpandsListWithBlanks()
    {
        _sut.SaveRoute(2, "?f=/deep");

        var routes = _sut.GetRoutes();
        Assert.AreEqual(3, routes.Count);
        Assert.AreEqual("?f=/", routes[0].Route);
        Assert.AreEqual("?f=/", routes[1].Route);
        Assert.AreEqual("?f=/deep", routes[2].Route);
    }

    [TestMethod]
    public void RemoveRoute_RemovesEntry()
    {
        _sut.SaveRoute(0, "?f=/a");
        _sut.SaveRoute(1, "?f=/b");

        _sut.RemoveRoute(0);

        var routes = _sut.GetRoutes();
        Assert.AreEqual(1, routes.Count);
        Assert.AreEqual("?f=/b", routes[0].Route);
    }

    [TestMethod]
    public void ClearAll_EmptiesList()
    {
        _sut.SaveRoute(0, "?f=/a");
        _sut.SaveRoute(1, "?f=/b");

        _sut.ClearAll();

        Assert.AreEqual(0, _sut.GetRoutes().Count);
    }

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
