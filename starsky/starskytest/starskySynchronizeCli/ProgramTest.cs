using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskysynchronizecli;
using starskytest.FakeCreateAn;

namespace starskytest.starskySynchronizeCli
{
	[TestClass]
	public class SynchronizeCliTest
	{
		[TestMethod]
		[Timeout(5000)]
		public void SynchronizeCliHelpVerbose()
		{
			var args = new List<string> {
				"-h","-v"
			}.ToArray();
			Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		[TestMethod]
		[Timeout(5000)]
		public void SynchronizeCliHelpTest()
		{
			var newImage = new CreateAnImage();
			var args = new List<string> {"-h","-v","-c","test","-d", "InMemoryDatabase", 
				"-b", newImage.BasePath, "--thumbnailtempfolder", 
				newImage.BasePath, "-e", newImage.FullFilePath 
			}.ToArray();
			Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		[TestMethod]
		[Timeout(5000)]
		public void SynchronizeCli_StarskyCliSubPathOneImage()
		{
			var newImage = new CreateAnImage();
			var args = new List<string> {
				"-v",
				"-c","test",
				"-d", "InMemoryDatabase", 
				"-b", newImage.BasePath, 
				"--thumbnailtempfolder", newImage.BasePath, 
				"--exiftoolpath", newImage.FullFilePath 
			}.ToArray();
            
			foreach (var arg in args)
			{
				Console.WriteLine(arg);
			}
            
			Program.Main(args);
			Assert.IsNotNull(args);
		}
        
	}
}
