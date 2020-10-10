using System;
using System.Collections.Generic;
using System.IO;
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
using starsky.foundation.writemeta.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Helpers
{
	[TestClass]
	public class ExifToolDownloadTest
	{
		private IServiceScopeFactory _serviceScopeFactory;
		private AppSettings _appSettings;
		private IStorage _hostFileSystem;

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
			_appSettings = new AppSettings{TempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"temp")};
			
			_hostFileSystem  = new StorageHostFullPathFilesystem();
			_hostFileSystem.CreateDirectory(_appSettings.TempFolder);
		}

		[TestMethod]
		public void GetUnixTarGzFromChecksum()
		{
			var result = new ExifToolDownload(null, _appSettings).GetUnixTarGzFromChecksum(ExampleCheckSum);
			Assert.AreEqual("Image-ExifTool-11.99.tar.gz",result);
		}
		
		[TestMethod]
		public void GetWindowsZipFromChecksum()
		{
			var result = new ExifToolDownload(null, _appSettings).GetWindowsZipFromChecksum(ExampleCheckSum);
			Assert.AreEqual("exiftool-11.99.zip",result);
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
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			var result = await new ExifToolDownload(httpClientHelper,_appSettings ).DownloadExifTool(_appSettings.IsWindows);
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
		
		[TestMethod]
		public async Task RunChmodOnExifToolUnixExe_TempFolderWithSpace_UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Console.WriteLine("This test is for unix only");
				return;
			}
			var appSettings = new AppSettings{TempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test temp")};
			
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new ByteArrayContent(CreateAnExifToolTarGz.Bytes)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
			Directory.CreateDirectory(appSettings.TempFolder);
			Directory.CreateDirectory(Path.Combine(appSettings.TempFolder,"exiftool-unix"));

			var stream = new PlainTextFileHelper().StringToStream("#!/bin/bash");
			await new StorageHostFullPathFilesystem().WriteStreamAsync(stream,
				Path.Combine(appSettings.TempFolder, "exiftool-unix", "exiftool"));
			
			Console.WriteLine(appSettings.TempFolder);
			var exifToolDownload = new ExifToolDownload(httpClientHelper, appSettings);
			var result = await exifToolDownload.RunChmodOnExifToolUnixExe();
			Assert.IsTrue(result);

			var lsLah = await Command.Run("ls", "-lah",
				Path.Combine(appSettings.TempFolder, "exiftool-unix", "exiftool")).Task;
			
			Console.WriteLine(lsLah.StandardOutput);
			
			Assert.IsTrue(lsLah.StandardOutput.StartsWith("-rwxr-xr-x"));
			Directory.Delete(appSettings.TempFolder,true);
		}
		
		[TestMethod]
		public async Task DownloadExifTool_Windows()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), _serviceScopeFactory);

			var result = await new ExifToolDownload(httpClientHelper,_appSettings ).DownloadExifTool(true);
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task DownloadExifTool_Unix()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(), _serviceScopeFactory);

			var result = await new ExifToolDownload(httpClientHelper,_appSettings ).DownloadExifTool(false);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task StartDownloadForWindows_2Times()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)}
			});

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
			var result = await new ExifToolDownload(httpClientHelper,_appSettings ).StartDownloadForWindows();
			Assert.IsTrue(result);
			
			// And run again
			// ByteArray content is Disposed afterwards
			var fakeIHttpProvider2 = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/exiftool-11.99.zip", new ByteArrayContent(CreateAnExifToolWindows.Bytes)}
			});
			var httpClientHelper2 = new HttpClientHelper(fakeIHttpProvider2, _serviceScopeFactory);
			var result2 = await new ExifToolDownload(httpClientHelper2,_appSettings ).StartDownloadForWindows();
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

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
			await new ExifToolDownload(httpClientHelper, _appSettings).
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

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
			await new ExifToolDownload(httpClientHelper, _appSettings).
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

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
			var result = await new ExifToolDownload(httpClientHelper,_appSettings ).StartDownloadForUnix();
			Assert.IsTrue(result);
			
			// And run again
			// ByteArray content is Disposed afterwards
			var fakeIHttpProvider2 = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{"https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum)},
				{"https://exiftool.org/Image-ExifTool-11.99.tar.gz", new ByteArrayContent(CreateAnExifToolTarGz.Bytes)}
			});
			var httpClientHelper2 = new HttpClientHelper(fakeIHttpProvider2, _serviceScopeFactory);
			var result2 = await new ExifToolDownload(httpClientHelper2,_appSettings ).StartDownloadForUnix();
			Assert.IsTrue(result2);

			_hostFileSystem.FolderDelete(ExifToolUnixTempPath);
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
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
			var result = await new ExifToolDownload(httpClientHelper,_appSettings ).StartDownloadForUnix();
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
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
			var result = await new ExifToolDownload(httpClientHelper,_appSettings ).StartDownloadForWindows();
			Assert.IsFalse(result);
		}
	}
}
