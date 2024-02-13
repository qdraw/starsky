using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers;

[TestClass]
public class WindowsOpenDesktopAppTests
{
	[TestMethod]
	public async Task OpenDefault()
	{
		WindowsOpenDesktopApp.OpenDefault("test");

		ProcessStartInfo psi = new ProcessStartInfo();
		psi.FileName = "C:\\testcontent\\20221029_101722_DSC05623.arw";
		psi.UseShellExecute = true;
		psi.WindowStyle = ProcessWindowStyle.Normal;
		Process.Start(psi);
	}
}

