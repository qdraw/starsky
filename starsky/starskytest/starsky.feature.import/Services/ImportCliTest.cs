using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.import.Services
{
	[TestClass]
	public class ImportCliTest
	{
		private readonly FakeIStorage _iStorageFake;
		private readonly string _exampleHash;
		private readonly FakeIStorage _iStorageDirectoryRecursive;

		[TestMethod]
		public async Task ImporterCli_NoArgs_DefaultHelp()
		{
			var fakeConsole = new FakeConsoleWrapper(new List<string>());
			await new ImportCli().Importer(new List<string>().ToArray(), 
				new FakeIImport(new FakeSelectorStorage()), new AppSettings(), fakeConsole);
			Assert.IsTrue(fakeConsole.WrittenLines.FirstOrDefault().Contains("Starksy Importer Cli ~ Help"));
		}
		
		[TestMethod]
		public async Task ImporterCli_ArgPath()
		{
			var fakeConsole = new FakeConsoleWrapper(new List<string>());
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test"}, 
				new List<byte[]>(new byte[0][]));
			
			await new ImportCli().Importer(new List<string>{"-p", "/test"}.ToArray(), 
				new FakeIImport(new FakeSelectorStorage(storage)), new AppSettings(), fakeConsole);
			Assert.IsTrue(fakeConsole.WrittenLines.FirstOrDefault().Contains("Done Importing"));
		}
		
		[TestMethod]
        public async Task ImporterCli_ArgPath_Verbose()
        {
        	var fakeConsole = new FakeConsoleWrapper(new List<string>());
        	var storage = new FakeIStorage(new List<string>{"/"}, 
        		new List<string>{"/test"}, 
        		new List<byte[]>(new byte[0][]));
        	
        	await new ImportCli().Importer(new List<string>{"-p", "/test"}.ToArray(), 
        		new FakeIImport(new FakeSelectorStorage(storage)), new AppSettings{Verbose = true}, fakeConsole);
        	Assert.IsTrue(fakeConsole.WrittenLines.LastOrDefault().Contains("Failed"));
        }
        		
	}
}
