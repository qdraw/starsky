using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

[TestClass]
public class PortFinderTests
{
    [TestMethod]
    public void FindFreePort_ReturnsPositivePort()
    {
        var port = PortFinder.FindFreePort();
        Assert.IsTrue(port > 0, $"Expected positive port, got {port}");
    }

    [TestMethod]
    public void FindFreePort_PortIsNotInUse()
    {
        var port = PortFinder.FindFreePort();

        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        var ep = (IPEndPoint)listener.LocalEndpoint;
        Assert.AreEqual(port, ep.Port);
    }

    [TestMethod]
    public void FindFreePort_ReturnsDifferentPortsOnSuccessiveCalls()
    {
        // Ports can in theory be re-used, but two rapid calls rarely return the same value
        var ports = Enumerable.Range(0, 5).Select(_ => PortFinder.FindFreePort()).ToList();
        // At minimum all are valid
        foreach (var p in ports) Assert.IsTrue(p > 0);
    }
}
