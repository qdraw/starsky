using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.import.Services
{
	[TestClass]
	public class ImportCliTest
	{
		private readonly HttpClientHelper _httpClientHelper;

		public ImportCliTest()
		{
			_httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), null);
		}

		[TestMethod]
		public async Task ImporterCli_CheckIfExifToolIsCalled()
		{
			var fakeExifToolDownload = new FakeExifToolDownload();
			
			var fakeConsole = new FakeConsoleWrapper(new List<string>());
			await new ImportCli( 
				new FakeIImport(new FakeSelectorStorage()), new AppSettings{TempFolder = "/___not___found_"},
				fakeConsole, fakeExifToolDownload).Importer(new List<string>().ToArray());

			Assert.IsTrue(fakeExifToolDownload.Called.Any());
		}

		[TestMethod]
		public async Task ImporterCli_NoArgs_DefaultHelp()
		{
			var fakeConsole = new FakeConsoleWrapper(new List<string>());
			await new ImportCli( 
				new FakeIImport(new FakeSelectorStorage()), new AppSettings(),
				fakeConsole, new FakeExifToolDownload()).Importer(new List<string>().ToArray());
			
			Assert.IsTrue(fakeConsole.WrittenLines.FirstOrDefault().Contains("Starksy Importer Cli ~ Help"));
		}
		
		[TestMethod]
		public async Task ImporterCli_ArgPath()
		{
			var fakeConsole = new FakeConsoleWrapper(new List<string>());
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test"}, 
				new List<byte[]>(new byte[0][]));
			
			await new ImportCli(new FakeIImport(new FakeSelectorStorage(storage)), 
				new AppSettings(), fakeConsole, new FakeExifToolDownload()).Importer(
				new List<string>{"-p", "/test"}.ToArray());
			Assert.IsTrue(fakeConsole.WrittenLines.FirstOrDefault().Contains("Done Importing"));
		}
		
		[TestMethod]
		public async Task ImporterCli_ArgPath_Verbose()
		{
			var fakeConsole = new FakeConsoleWrapper(new List<string>());
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test"}, 
				new List<byte[]>(new byte[0][]));
			
			var cli = new ImportCli(new FakeIImport(new FakeSelectorStorage(storage)), 
				new AppSettings {Verbose = true}, fakeConsole, new FakeExifToolDownload());
				
			// verbose is entered here 
			await cli.Importer(new List<string>{"-p", "/test", "-v", "true"}.ToArray());
			
			Assert.IsTrue(fakeConsole.WrittenLines.LastOrDefault().Contains("Failed: 2"));
		}
		
		[TestMethod]
		public async Task ImporterCli_ArgPath_Failed()
		{
			var fakeConsole = new FakeConsoleWrapper(new List<string>());
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test"}, 
				new List<byte[]>(new byte[0][]));
        	
			await new ImportCli(new FakeIImport(new FakeSelectorStorage(storage)), 
					new AppSettings{Verbose = false}, fakeConsole, new FakeExifToolDownload())
				.Importer(new List<string>{"-p", "/test"}.ToArray());
			Assert.IsTrue(fakeConsole.WrittenLines.LastOrDefault().Contains("Failed"));
		}
        		
	}
}
