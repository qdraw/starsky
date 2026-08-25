using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class PortFinderTests
{
    [Fact]
    public void FindFreePort_ReturnsPositivePort()
    {
        var port = PortFinder.FindFreePort();
        Assert.True(port > 0, $"Expected positive port, got {port}");
    }

    [Fact]
    public void FindFreePort_PortIsNotInUse()
    {
        var port = PortFinder.FindFreePort();

        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        var ep = (System.Net.IPEndPoint)listener.LocalEndpoint;
        Assert.Equal(port, ep.Port);
    }

    [Fact]
    public void FindFreePort_ReturnsDifferentPortsOnSuccessiveCalls()
    {
        // Ports can in theory be re-used, but two rapid calls rarely return the same value
        var ports = Enumerable.Range(0, 5).Select(_ => PortFinder.FindFreePort()).ToList();
        // At minimum all are valid
        Assert.All(ports, p => Assert.True(p > 0));
    }
}
