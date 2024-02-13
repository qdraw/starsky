using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.storage.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers
{
	[TestClass]
	public class WindowsGetDefaultAppByExtensionTests
	{
		[TestMethod]
		public void WindowsGetDefaultAppByExtension1()
		{

			//var filePath = "C:\\testcontent\\20221029_101722_DSC05623.jpg";
			//System.Diagnostics.Process proc = new System.Diagnostics.Process();
			//proc.EnableRaisingEvents = false;
			//proc.StartInfo.FileName = "rundll32.exe";
			//proc.StartInfo.Arguments = "shell32,OpenAs_RunDLL " + filePath;
			//proc.Start();


			WindowsGetDefaultAppByExtension.GetDefaultApp1();

			WindowsGetDefaultAppByExtension.GetDefaultApp3();
		}
	}
}
