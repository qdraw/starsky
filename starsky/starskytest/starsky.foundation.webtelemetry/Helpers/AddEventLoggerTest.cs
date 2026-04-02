using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

[TestClass]
public sealed class AddEventLoggerTest
{
    private sealed class FakeLoggingBuilder : ILoggingBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
        public bool AddProviderCalled { get; private set; }
        public ILoggerProvider? LastProvider { get; private set; }

        public void AddProvider(ILoggerProvider provider)
        {
            AddProviderCalled = true;
            LastProvider = provider;
        }
    }

    [TestMethod]
    public void AddEventLog_Windows_InvokesAddEventLog()
    {
        var builder = new FakeLoggingBuilder();
        var addEventLogger = new AddEventLogger(() => OSPlatform.Windows);

        var source = addEventLogger.AddEventLog(builder, AppSettings.StarskyAppType.WebController);

        Assert.AreEqual("nl.qdraw.webcontroller", source);
        // The EventLog extension registers services in the builder's IServiceCollection
        Assert.IsNotEmpty(builder.Services, "Expected AddEventLog to register services on Windows");
    }

    [TestMethod]
    public void AddEventLog_Linux_DoesNotInvokeAddEventLog()
    {
        var builder = new FakeLoggingBuilder();
        var addEventLogger = new AddEventLogger(() => OSPlatform.Linux);

        var source = addEventLogger.AddEventLog(builder, AppSettings.StarskyAppType.WebController);

        Assert.AreEqual("nl.qdraw.webcontroller", source);
        Assert.AreEqual(0, builder.Services.Count);
    }

    [TestMethod]
    public void AddEventLog_MacOS_DoesNotInvokeAddEventLog()
    {
        var builder = new FakeLoggingBuilder();
        var addEventLogger = new AddEventLogger(() => OSPlatform.OSX);

        var source = addEventLogger.AddEventLog(builder, AppSettings.StarskyAppType.WebController);

        Assert.AreEqual("nl.qdraw.webcontroller", source);
        Assert.AreEqual(0, builder.Services.Count);
    }
}

