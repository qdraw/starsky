using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Attributes;
using starskysynccli;
using starskytest.FakeCreateAn;

namespace starskytest.starskySyncCli
{
	[TestClass]
	public class StarskyCliTest
	{
		[TestMethod]
		public void SyncV1CliHelpVerbose()
		{
			Environment.SetEnvironmentVariable("app__databaseType","InMemoryDatabase");
			Environment.SetEnvironmentVariable("app__databaseConnection", "env_test");
			
			var args = new List<string> {
				"-h","-v"
			}.ToArray();
			Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		[ExcludeFromCoverage]
		[TestMethod]
		[Timeout(5000)]
		public void SyncV1CliHelpTest()
		{
			Environment.SetEnvironmentVariable("app__databaseType","InMemoryDatabase");
			Environment.SetEnvironmentVariable("app__databaseConnection", "env_test");
			
			var newImage = new CreateAnImage();
			var args = new List<string> {"-h","-v","-c","test","-d", "InMemoryDatabase", 
				"-b", newImage.BasePath, "--thumbnailtempfolder", 
				newImage.BasePath, "-e", newImage.FullFilePath 
			}.ToArray();
			Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		[ExcludeFromCoverage]
		[TestMethod]
		[Timeout(10000)]
		public void SyncV1Cli_StarskyCliSubPathOneImage()
		{
			Environment.SetEnvironmentVariable("app__databaseType","InMemoryDatabase");
			Environment.SetEnvironmentVariable("app__databaseConnection", "env_test");
			
			var newImage = new CreateAnImage();
			var args = new List<string> {
				"-v",
				"-c","test",
				"--connection", "StarskyCliSubPathOneImage",
				"-d", "InMemoryDatabase", 
				"-b", newImage.BasePath, 
				"--thumbnailtempfolder", newImage.BasePath, 
				"--exiftoolpath", newImage.FullFilePath,
				"-s", newImage.DbPath
			}.ToArray();

			Console.WriteLine("SyncV1Cli_StarskyCliSubPathOneImage -->");
			Console.WriteLine("-->");
			foreach (var arg in args)
			{
				Console.WriteLine(arg);
			}
            
			Program.Main(args);
			Console.WriteLine("<--");
			Console.WriteLine("<-- SyncV1Cli_StarskyCliSubPathOneImage");
			Assert.IsNotNull(args);
		}
        
	}
}
