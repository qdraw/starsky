using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Attributes;
using starskyGeoCli;
using starskytest.FakeCreateAn;

namespace starskytest.starskyGeoCli
{
	[TestClass]
	public class starskyGeoCliTest
	{
		[ExcludeFromCoverage]
		[TestMethod]
		public void StarskyGeoCli_HelpVerbose()
		{
			var args = new List<string> {
				"-h","-v"
			}.ToArray();
			Program.Main(args);
		}
        
		[ExcludeFromCoverage]
		[TestMethod]
		public void StarskyGeoCli_HelpTest()
		{
			var newImage = new CreateAnImage();
			var args = new List<string> {"-h","-v","-c","test","-d", "InMemoryDatabase", 
				"-b", newImage.BasePath, "--thumbnailtempfolder", 
				newImage.BasePath, "-e", newImage.FullFilePath 
			}.ToArray();
			Program.Main(args);
		}
        
		[ExcludeFromCoverage]
		[TestMethod]
		public void StarskyGeoCli_SubPathOneImage()
		{
			var newImage = new CreateAnImage();
			var args = new List<string> {
				"-v",
				"-c","test",
				"-d", "InMemoryDatabase", 
				"-p", newImage.BasePath, 
				"--thumbnailtempfolder", newImage.BasePath, 
				"--exiftoolpath", newImage.FullFilePath 
			}.ToArray();
            
			foreach (var arg in args)
			{
				Console.WriteLine(arg);
			}
            
			Program.Main(args);
		}
	
        
	}
}
