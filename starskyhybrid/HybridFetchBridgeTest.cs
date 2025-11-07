using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyhybrid;
using System.Threading.Tasks;

[TestClass]
public class HybridFetchBridgeTest
{
    [TestMethod]
    public async Task HandleApiCallAsync_UserGet_ReturnsUser()
    {
        var bridge = new HybridFetchBridge();
        var req = new HybridFetchBridge.FetchRequest
        {
            Type = "api",
            Url = "/api/user",
            Method = "GET"
        };
        var result = await bridge.HandleApiCallAsync(req);
        Assert.AreEqual("Dion", ((dynamic)result).name);
    }

    [TestMethod]
    public async Task HandleApiCallAsync_LoginPost_ReturnsSuccess()
    {
        var bridge = new HybridFetchBridge();
        var req = new HybridFetchBridge.FetchRequest
        {
            Type = "api",
            Url = "/api/login",
            Method = "POST",
            Body = "{\"username\":\"dion\",\"password\":\"test123\"}"
        };
        var result = await bridge.HandleApiCallAsync(req);
        Assert.IsTrue(((dynamic)result).success);
    }

    [TestMethod]
    public async Task HandleApiCallAsync_Unknown_ReturnsError()
    {
        var bridge = new HybridFetchBridge();
        var req = new HybridFetchBridge.FetchRequest
        {
            Type = "api",
            Url = "/api/unknown",
            Method = "GET"
        };
        var result = await bridge.HandleApiCallAsync(req);
        Assert.AreEqual("Unknown endpoint", ((dynamic)result).error);
    }
}
