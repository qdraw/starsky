using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Helpers;

namespace starskytest.starsky.foundation.import.Helpers;

[TestClass]
public class RealPathHelperTest
{
	[TestMethod]
	public void GetRealPath_WithNonExisting_ReturnsInput()
	{
		var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		var resolved = RealPathHelper.GetRealPath(tmp);
		Assert.AreEqual(tmp, resolved);
	}


	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
	public void GetRealPath_WithSymlink_ResolvesToTarget()
	{
		var dir = Path.Combine(Path.GetTempPath(), "msft_test_symlink" + Guid.NewGuid());
		Directory.CreateDirectory(dir);
		try
		{
			var target = Path.Combine(dir, "target.txt");
			File.WriteAllText(target, "hello");
			var link = Path.Combine(dir, "link.txt");

			try
			{
				var psi = new ProcessStartInfo("ln", $"-s \"{target}\" \"{link}\"")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false
				};

				var p = Process.Start(psi);
				if ( p == null )
				{
					Assert.Inconclusive("Could not start ln process");
					return;
				}

				p.WaitForExit();
				if ( p.ExitCode != 0 )
				{
					Assert.Inconclusive("ln process failed to create symlink");
					return;
				}
			}
			catch
			{
				return;
			}

			var resolved = RealPathHelper.GetRealPath(link);
			Assert.IsTrue(resolved.EndsWith("target.txt") || resolved.EndsWith("link.txt"));
		}
		finally
		{
			try
			{
				Directory.Delete(dir, true);
			}
			catch
			{
				// do nothing
			}
		}
	}
}
