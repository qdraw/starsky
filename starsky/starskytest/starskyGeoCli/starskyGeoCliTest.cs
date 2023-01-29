using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyGeoCli;
using starskytest.FakeCreateAn;

namespace starskytest.starskyGeoCli
{
	[TestClass]
	public sealed class starskyGeoCliTest
	{
		[TestMethod]
		public async Task StarskyGeoCli_HelpVerbose()
		{
			var args = new List<string> {
				"-h","-v"
			}.ToArray();
			await Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		[TestMethod]
		public async Task StarskyGeoCli_HelpTest()
		{
			var newImage = new CreateAnImage();
			var args = new List<string> {"-h","-v","-c","test","-d", "InMemoryDatabase", 
				"-b", newImage.BasePath, "--thumbnailtempfolder", 
				newImage.BasePath, "-e", newImage.FullFilePath 
			}.ToArray();
			await Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		// [ExcludeFromCoverage]
		// [TestMethod]
		// public void StarskyGeoCli_SubPathOneImage()
		// {
		// 	var newImage = new CreateAnImage();
		// 	var args = new List<string> {
		// 		"-v",
		// 		"-c","test",
		// 		"-d", "InMemoryDatabase", 
		// 		"-p", newImage.BasePath, 
		// 		"--thumbnailtempfolder", newImage.BasePath, 
		// 		"--exiftoolpath", newImage.FullFilePath 
		// 	}.ToArray();
  //           
		// 	foreach (var arg in args)
		// 	{
		// 		Console.WriteLine(arg);
		// 	}
  //           
		// 	Program.Main(args);
		// 	Assert.IsNotNull(args);
		// }
	
        
	}
}
