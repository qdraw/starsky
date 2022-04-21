using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Helpers
{
	[TestClass]
	public class ExifToolDownloadTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly AppSettings _appSettings;
		private readonly IStorage _hostFileSystem;

		/// <summary>
		/// shasum -a 1 file.zip
		/// </summary>
		private static readonly string ExampleCheckSum =
			"SHA1(Image-ExifTool-11.99.tar.gz)= "+ CreateAnExifToolTarGz.Sha1 + "\n" +
			"SHA1(exiftool-11.99.zip)= " + CreateAnExifToolWindows.Sha1 +"\n" +
			"SHA1(ExifTool-11.99.dmg)= 3d30a4846eab278387be51b91ef4121916375ded\n" +
			"MD5 (Image-ExifTool-11.99.tar.gz) = 06b97602e0d71dc413863a905708f0c9\n" +
			"MD5 (exiftool-11.99.zip) = 19b53eede582e809c115b69e83dbac5e\n" +
			"MD5 (ExifTool-11.99.dmg) = d063809eb7ac35e0d6c6cea6e829f75a";
		
		private string ExifToolUnixTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp",
			"exiftool-unix");
		
		private string ExifToolWindowsTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp",
			"exiftool-windows");
		
		public ExifToolDownloadTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, StorageHostFullPathFilesystem>();
			services.AddSingleton<ISelectorStorage, SelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			_appSettings = new AppSettings
			{
				TempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"),
				Verbose = true
			};

			_hostFileSystem  = new StorageHostFullPathFilesystem();
			_hostFileSystem.CreateDirectory(_appSettings.TempFolder);
		}

		[TestMethod]
		public void GetUnixTarGzFromChecksum()
		{
			var result = ExifToolDownload.GetUnixTarGzFromChecksum(ExampleCheckSum);
			Assert.AreEqual("Image-ExifTool-11.99.tar.gz",result);
		}
		
		[TestMethod]
		public void GetWindowsZipFromChecksum()
		{
			var result = ExifToolDownload.GetWindowsZipFromChecksum(ExampleCheckSum);
			Assert.AreEqual("exiftool-11.99.zip",result);
		}

		[TestMethod]
		public async Task DownloadCheckSums_BaseChecksumDoesExist()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			// Happy flow
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() )
				.DownloadCheckSums();
			Assert.AreEqual(ExampleCheckSum, result.Value.Value);
			Assert.AreEqual(true, result.Value.Key);
		}
		
		[TestMethod]
		public async Task DownloadCheckSums_BaseChecksumDoesNotExist()
		{
			// Main source is down, but mirror is up
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://qdraw.nl/special/mirror/exiftool/checksums.txt", new StringContent(ExampleCheckSum)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			// Main source is down, but mirror is up
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() )
				.DownloadCheckSums();
			
			Assert.AreEqual(ExampleCheckSum, result.Value.Value);
			Assert.AreEqual(false, result.Value.Key);
		}
		
		[TestMethod]
		public async Task DownloadCheckSums_BothServicesAreDown()
		{
			// Main & Mirror source are down
			var fakeIHttpProvider = new FakeIHttpProvider();
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			// Main & Mirror source are down
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() )
				.DownloadCheckSums();
			
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public async Task GetExifToolByOs()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new ByteArrayContent(CreateAnExifToolTarGz.Bytes)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() ).DownloadExifTool(_appSettings.IsWindows);
			Assert.IsTrue(result);

			if ( _hostFileSystem.ExistFolder(ExifToolWindowsTempPath) )
			{
				_hostFileSystem.FolderDelete(ExifToolWindowsTempPath);
			}
			if ( _hostFileSystem.ExistFolder(ExifToolUnixTempPath) )
			{
				_hostFileSystem.FolderDelete(ExifToolUnixTempPath);
			}
		}

		private static async Task<AppSettings> CreateTempFolderWithExifTool(string name = "test temp")
		{
			var appSettings = new AppSettings{TempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,name)};
			Directory.CreateDirectory(appSettings.TempFolder);
			Directory.CreateDirectory(Path.Combine(appSettings.TempFolder,"exiftool-unix"));
			var stream = new PlainTextFileHelper().StringToStream("#!/bin/bash");
			try
			{
				await new StorageHostFullPathFilesystem().WriteStreamAsync(stream,
					Path.Combine(appSettings.TempFolder, "exiftool-unix", "exiftool"));
			}
			catch ( ArgumentOutOfRangeException e )
			{
				Console.WriteLine(e);
				throw;
			}
			
			return appSettings;
		}
		
		[TestMethod]
		public async Task RunChmodOnExifToolUnixExe_TempFolderWithSpace_UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Console.WriteLine("This test is for unix only");
				return;
			}
			
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new ByteArrayContent(CreateAnExifToolTarGz.Bytes)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			var appSettings = await CreateTempFolderWithExifTool();
			
			Console.WriteLine(appSettings.TempFolder);
			var exifToolDownload = new ExifToolDownload(httpClientHelper, appSettings, new FakeIWebLogger());
			var result = await exifToolDownload.RunChmodOnExifToolUnixExe();
			Assert.IsTrue(result);

			var lsLah = await Command.Run("ls", "-lah",
				Path.Combine(appSettings.TempFolder, "exiftool-unix", "exiftool")).Task;
			
			Console.WriteLine(lsLah.StandardOutput);
			
			Assert.IsTrue(lsLah.StandardOutput.StartsWith("-rwxr-xr-x"));
			Directory.Delete(appSettings.TempFolder,true);
		}
		
		[TestMethod]
		public async Task RunChmodOnExifToolUnixExe_Chmod644_UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Console.WriteLine("This test is for unix only");
				return;
			}
			
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new ByteArrayContent(CreateAnExifToolTarGz.Bytes)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			await CreateTempFolderWithExifTool("temp");
			
			// make NOT executable 644 
			await Command.Run("chmod", "644",
				Path.Combine(_appSettings.TempFolder, "exiftool-unix", "exiftool")).Task;

			var exifToolDownload = new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger());
			var result = await exifToolDownload.RunChmodOnExifToolUnixExe();
			Assert.IsTrue(result);


			Directory.Delete(_appSettings.TempFolder,true);
		}
		
		[TestMethod]
		public async Task DownloadExifTool_Windows()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), _serviceScopeFactory, new FakeIWebLogger());

			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() ).DownloadExifTool(true);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task DownloadExifTool_Skip_AddSwaggerExportExitAfter()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), _serviceScopeFactory, new FakeIWebLogger());

			var appSettings = new AppSettings
			{
				AddSwaggerExport = true,
				AddSwaggerExportExitAfter = true
			};
			var logger = new FakeIWebLogger();
			var result = await new ExifToolDownload(httpClientHelper,appSettings,  logger).DownloadExifTool(true);
			Assert.IsFalse(result);
			Assert.IsTrue(logger.TrackedInformation[0].Item2.Contains("Skipped due AddSwaggerExportExitAfter setting"));
		}

		[TestMethod]
		public async Task DownloadExifTool_Unix()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), _serviceScopeFactory, new FakeIWebLogger());
			Directory.Delete(_appSettings.TempFolder,true);
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() ).DownloadExifTool(false);
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task DownloadExifTool_Windows_existVerbose()
		{
			var appSettings = new AppSettings{
				TempFolder = AppDomain.CurrentDomain.BaseDirectory,
				Verbose = true
			};
			Directory.CreateDirectory(appSettings.TempFolder);
			Directory.CreateDirectory(Path.Combine(appSettings.TempFolder,"exiftool-windows"));
			
			var stream = new PlainTextFileHelper().StringToStream("#!/bin/bash");
			await new StorageHostFullPathFilesystem().WriteStreamAsync(stream,
				Path.Combine(appSettings.TempFolder, "exiftool-windows", "exiftool.exe"));
			
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), _serviceScopeFactory, new FakeIWebLogger());
			var logger = new FakeIWebLogger();
			
			var result = await new ExifToolDownload(httpClientHelper,appSettings,logger).DownloadExifTool(true);
			
			Assert.IsTrue(result);
			Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2
				.Contains("[DownloadExifTool] " + appSettings.TempFolder));
			
			Directory.Delete(Path.Combine(appSettings.TempFolder,"exiftool-windows"),true);
		}
		
				
		[TestMethod]
		public async Task DownloadExifTool_Unix_existVerbose()
		{
			if ( _appSettings.IsWindows )
			{
				Console.WriteLine("This test is for unix only");
				return;
			}
			
			var appSettings = await CreateTempFolderWithExifTool("test32");
			appSettings.Verbose = true;
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), _serviceScopeFactory, new FakeIWebLogger());
			var logger = new FakeIWebLogger();
			var result = await new ExifToolDownload(httpClientHelper,appSettings,logger).DownloadExifTool(false);
			
			Assert.IsTrue(result);
			Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2
				.Contains("[DownloadExifTool] " + appSettings.TempFolder));
			
			Directory.Delete(appSettings.TempFolder,true);
		}
		
		

		[TestMethod]
		public async Task StartDownloadForWindows_2Times()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)}
			});

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() ).StartDownloadForWindows();
			Assert.IsTrue(result);
			
			// And run again
			// ByteArray content is Disposed afterwards
			var fakeIHttpProvider2 = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)}
			});
			var httpClientHelper2 = new HttpClientHelper(fakeIHttpProvider2, _serviceScopeFactory, new FakeIWebLogger());
			var result2 = await new ExifToolDownload(httpClientHelper2,_appSettings, new FakeIWebLogger() ).StartDownloadForWindows();
			Assert.IsTrue(result2);
			
			_hostFileSystem.FolderDelete(ExifToolWindowsTempPath);
		}

		[TestMethod]
		[ExpectedException(typeof(HttpRequestException))]
		public async Task StartDownloadForWindows_Fail()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
			});

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger()).
				StartDownloadForWindows();
		}
		
		[TestMethod]
		[ExpectedException(typeof(HttpRequestException))]
		public async Task StartDownloadForUnix_Fail()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
			});

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger()).
				StartDownloadForUnix();
		}

		[TestMethod]
		public async Task StartDownloadForUnix_2Times()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new ByteArrayContent(CreateAnExifToolTarGz.Bytes)}
			});

			_appSettings.Verbose = true;

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() ).StartDownloadForUnix();
			Assert.IsTrue(result);
			
			// And run again
			// ByteArray content is Disposed afterwards
			var fakeIHttpProvider2 = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new ByteArrayContent(CreateAnExifToolTarGz.Bytes)}
			});
			var httpClientHelper2 = new HttpClientHelper(fakeIHttpProvider2, _serviceScopeFactory, new FakeIWebLogger());
			var result2 = await new ExifToolDownload(httpClientHelper2,_appSettings, new FakeIWebLogger() ).StartDownloadForUnix();
			Assert.IsTrue(result2);

			_hostFileSystem.FolderDelete(ExifToolUnixTempPath);
		}

		[TestMethod]
		public void CheckSha1_Good()
		{
			var fakeIStorage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/exiftool.exe"},
				new List<byte[]> {CreateAnExifToolTarGz.Bytes});
			
			var result2 = new ExifToolDownload(null,_appSettings, new FakeIWebLogger(), fakeIStorage)
				.CheckSha1("/exiftool.exe", new List<string>{CreateAnExifToolTarGz.Sha1});
			Assert.IsTrue(result2);
		}
		
		[TestMethod]
		public void CheckSha1_Bad()
		{
			var fakeIStorage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/exiftool.exe"},
				new List<byte[]> {CreateAnExifToolTarGz.Bytes});
			
			var result2 = new ExifToolDownload(null,_appSettings, new FakeIWebLogger(), fakeIStorage)
				.CheckSha1("/exiftool.exe", new List<string>{"random_value"});
			Assert.IsFalse(result2);
		}

		[TestMethod]
		[ExpectedException(typeof(HttpRequestException))]
		public async Task StartDownloadForUnix_WrongHash()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new StringContent("FAIL")}
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() ).StartDownloadForUnix();
			Assert.IsFalse(result);
		}

		[TestMethod]
		[ExpectedException(typeof(HttpRequestException))]
		public async Task StartDownloadForWindows_WrongHash()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{
					"https://exiftool.org/exiftool-11.99.zip", new StringContent("FAIL")
				}
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());
			var result = await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() ).StartDownloadForWindows();
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task DownloadForUnix_FromMirrorInsteadOfMainSource()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://qdraw.nl/special/mirror/exiftool/exiftool-11.99.zip", new StringContent("FAIL")},
				{"https://qdraw.nl/special/mirror/exiftool/Image-ExifTool-11.99.tar.gz", new StringContent("FAIL")}
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			try
			{
				await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() )
					.DownloadForUnix("Image-ExifTool-11.99.tar.gz", new List<string>().ToArray(), true);
			}
			catch ( HttpRequestException httpRequestException )
			{
				// Expected:<checksum for ---/Debug/netcoreapp3.1/temp/exiftool.tar.gz is not valid
				Assert.IsTrue(httpRequestException.Message.Contains("checksum for "));
				Assert.IsTrue(httpRequestException.Message.Contains("is not valid"));
				return;
			}
			throw new HttpRequestException("This test should hit the catch");
		}
		
		
		[TestMethod]
		public async Task DownloadForWindows_FromMirrorInsteadOfMainSource()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://qdraw.nl/special/mirror/exiftool/exiftool-11.99.zip", new StringContent("FAIL")},
				{"https://qdraw.nl/special/mirror/exiftool/Image-ExifTool-11.99.tar.gz", new StringContent("FAIL")}
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			try
			{
				await new ExifToolDownload(httpClientHelper,_appSettings, new FakeIWebLogger() )
					.DownloadForWindows("exiftool-11.99.zip", new List<string>().ToArray(), true);
			}
			catch ( HttpRequestException httpRequestException )
			{
				// Expected:<checksum for ---/Debug/netcoreapp3.1/temp/exiftool.tar.gz is not valid
				Assert.IsTrue(httpRequestException.Message.Contains("checksum for "));
				Assert.IsTrue(httpRequestException.Message.Contains("is not valid"));
				return;
			}
			throw new HttpRequestException("This test should hit the catch");
		}

		[TestMethod]
		public void GetChecksumsFromTextFile_ToManyShaResults()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>());

			var example =
				"SHA1(Image-ExifTool-12.40.tar.gz)= 09f3bee6491251390028580eb8af990e79674ada\n" +
				"SHA1(exiftool-12.40.zip)= 9428bb167512a8eec5891d3b8d2341427688f2f8\n" +
				"SHA1(ExifTool-12.40.dmg)= e20ed19da096807774b68cf8c15e29f1903ca641\n" +
				"MD5 (Image-ExifTool-12.40.tar.gz) = 72b40d69cf518edebbf5b661465950e7\n" +
				"MD5 (exiftool-12.40.zip) = fc834fd43d79da19fcb6461fb791b275\n" +
				"MD5 (ExifTool-12.40.dmg) = b30e391a4b53564de60a72f4347cade4\n";
			
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			var exifToolDownload = new ExifToolDownload(httpClientHelper, new AppSettings(),
				new FakeIWebLogger(), new FakeIStorage());
			var result = exifToolDownload.GetChecksumsFromTextFile(example,2);
			Assert.AreEqual(0, result.Length);
		}
		
		
		[TestMethod]
		public void GetChecksumsFromTextFile_Good()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>());

			var example =
				"SHA1(Image-ExifTool-12.40.tar.gz)= 09f3bee6491251390028580eb8af990e79674ada\n" +
				"SHA1(exiftool-12.40.zip)= 9428bb167512a8eec5891d3b8d2341427688f2f8\n" +
				"SHA1(ExifTool-12.40.dmg)= e20ed19da096807774b68cf8c15e29f1903ca641\n" +
				"MD5 (Image-ExifTool-12.40.tar.gz) = 72b40d69cf518edebbf5b661465950e7\n" +
				"MD5 (exiftool-12.40.zip) = fc834fd43d79da19fcb6461fb791b275\n" +
				"MD5 (ExifTool-12.40.dmg) = b30e391a4b53564de60a72f4347cade4\n";
			
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger());

			var exifToolDownload = new ExifToolDownload(httpClientHelper, new AppSettings(),
				new FakeIWebLogger(), new FakeIStorage());
			var result = exifToolDownload.GetChecksumsFromTextFile(example);
			Assert.AreEqual(3, result.Length);
		}
 	}
}
