using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class MacOsOpenUrlTests
{
    private const string ConsoleApp = "/System/Applications/Utilities/Console.app";
    private const string ConsoleName = "Console";

    [TestMethod]
    public async Task TestMethodWithSpecificApp__MacOnly()
    {
	    if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
	    {
		    Assert.Inconclusive("This test if for Mac OS Only");
		    return;
	    }
	    
        var filePath = new CreateAnImage().FullFilePath;

        MacOsOpenUrl.OpenApplicationAtUrl(filePath, ConsoleApp);

        var isProcess = Process.GetProcessesByName(ConsoleName).Any(p => p.MainModule?.FileName.Contains(ConsoleApp) == true);
        for (var i = 0; i < 15; i++)
        {
            isProcess = Process.GetProcessesByName(ConsoleName).Any(p => p.MainModule?.FileName.Contains(ConsoleApp) == true);
            if (isProcess)
            {
                await Command.Run("osascript", "-e", "tell application \"Console\" to if it is running then quit").Task;
                break;
            }

            await Task.Delay(5);
        }

        Assert.IsTrue(isProcess);
    }

    [TestMethod]
    public void TestMethodWithDefaultApp__MacOnly()
    {
	    if ( OperatingSystemHelper.GetPlatform() != OSPlatform.OSX )
	    {
		    Assert.Inconclusive("This test if for Mac OS Only");
		    return;
	    }
	    
        var result = MacOsOpenUrl.OpenDefault("urlNotFound");
        Assert.IsFalse(result);
    }
}
