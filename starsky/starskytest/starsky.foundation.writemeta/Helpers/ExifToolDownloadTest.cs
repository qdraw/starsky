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
using starsky.foundation.platform.Interfaces;
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
	public sealed class ExifToolDownloadTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly AppSettings _appSettings;
		private readonly StorageHostFullPathFilesystem _hostFileSystem;

		/// <summary>
		/// shasum -a 1 file.zip
		/// </summary>
		private static readonly string ExampleCheckSum =
			"SHA256(Image-ExifTool-11.99.tar.gz)= " + CreateAnExifToolTarGz.Sha256 + "\n" +
			"SHA256(exiftool-12.94_32.zip)= e0521db2115b3ee07f531ed7e3f686c57fca23b742c8f88b387aef6b682a12fe\n" +
			$"SHA256(exiftool-11.99_64.zip)= {CreateAnExifToolWindows.Sha256}\n" +
			"SHA256(exiftool-11.99.zip)= " + CreateAnExifToolWindows.Sha1 + "\n" +
			"SHA256(ExifTool-11.99.dmg)= e0521db2115b3ee07f531ed7e3f686c57fca23b742c8f88b387aef6b682a12fe\n" +
			"SHA1(Image-ExifTool-11.99.tar.gz)= " + CreateAnExifToolTarGz.Sha1 + "\n" +
			"SHA1(exiftool-11.99.zip)= " + CreateAnExifToolWindows.Sha1 + "\n" +
			"SHA1(ExifTool-11.99.dmg)= 3d30a4846eab278387be51b91ef4121916375ded\n" +
			"MD5 (Image-ExifTool-11.99.tar.gz) = 06b97602e0d71dc413863a905708f0c9\n" +
			"MD5 (exiftool-11.99.zip) = 19b53eede582e809c115b69e83dbac5e\n" +
			"MD5 (ExifTool-11.99.dmg) = d063809eb7ac35e0d6c6cea6e829f75a";

		private readonly string _exifToolUnixTempFolderPath;

		private readonly string _exifToolWindowsTempFolderPath;
		private readonly CreateAnImage _createAnImage;

		public ExifToolDownloadTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, StorageHostFullPathFilesystem>();
			services.AddSingleton<ISelectorStorage, SelectorStorage>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();

			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			_createAnImage = new CreateAnImage();
			_appSettings = new AppSettings
			{
				DependenciesFolder =
					Path.Combine(_createAnImage.BasePath, "starsky-tmp-dependencies-4835793"),
				Verbose = true
			};
			_exifToolUnixTempFolderPath = Path.Combine(_createAnImage.BasePath,
				"starsky-tmp-dependencies-4835793", "exiftool-unix");
			_exifToolWindowsTempFolderPath = Path.Combine(_createAnImage.BasePath,
				"starsky-tmp-dependencies-4835793", "exiftool-windows");

			_hostFileSystem = new StorageHostFullPathFilesystem(new FakeIWebLogger());
			_hostFileSystem.CreateDirectory(_appSettings.DependenciesFolder);
		}

		[TestMethod]
		public void GetUnixTarGzFromChecksum()
		{
			var result = ExifToolDownload.GetUnixTarGzFromChecksum(ExampleCheckSum);
			Assert.AreEqual("Image-ExifTool-11.99.tar.gz", result);
		}

		[TestMethod]
		public void GetWindowsZipFromChecksum()
		{
			var result = ExifToolDownload.GetWindowsZipFromChecksum(ExampleCheckSum);
			Assert.AreEqual("exiftool-11.99_64.zip", result);
		}

		[TestMethod]
		public async Task DownloadCheckSums_BaseChecksumDoesExist()
		{
			var checkSumUrl = "https://exiftool.org/checksums.txt";
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{checkSumUrl , new StringContent(ExampleCheckSum) },
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			// Happy flow
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger())
					.DownloadCheckSums(checkSumUrl);
			
			Assert.AreEqual(ExampleCheckSum, result?.Value);
			Assert.IsTrue(result?.Key);
		}

		[TestMethod]
		public async Task DownloadCheckSums_BaseChecksumDoesNotExist()
		{
			// Main source is down, but mirror is up
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{
					"https://qdraw.nl/special/mirror/exiftool/checksums.txt",
					new StringContent(ExampleCheckSum)
				},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			// Main source is down, but mirror is up
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger())
					.DownloadCheckSums();

			Assert.AreEqual(ExampleCheckSum, result?.Value);
			Assert.IsTrue(result?.Key);
		}

		[TestMethod]
		public async Task DownloadCheckSums_BothServicesAreDown()
		{
			// Main & Mirror source are down
			var fakeIHttpProvider = new FakeIHttpProvider();
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			// Main & Mirror source are down
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger())
					.DownloadCheckSums();

			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public async Task GetExifToolByOs()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/exiftool-11.99_64.zip",
					new ByteArrayContent([.. CreateAnExifToolWindows.Bytes])
				},
				{
					"https://exiftool.org/Image-ExifTool-11.99.tar.gz",
					new ByteArrayContent([.. CreateAnExifToolTarGz.Bytes])
				},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var result = await new ExifToolDownload(httpClientHelper, _appSettings,
				new FakeIWebLogger()).DownloadExifTool(_appSettings.IsWindows);

			Assert.IsTrue(result);

			if ( _hostFileSystem.ExistFolder(_exifToolWindowsTempFolderPath) )
			{
				_hostFileSystem.FolderDelete(_exifToolWindowsTempFolderPath);
			}

			if ( _hostFileSystem.ExistFolder(_exifToolUnixTempFolderPath) )
			{
				_hostFileSystem.FolderDelete(_exifToolUnixTempFolderPath);
			}
		}

		private async Task<AppSettings> CreateTempFolderWithExifTool(string name = "test temp")
		{
			var appSettings = new AppSettings
			{
				DependenciesFolder = Path.Combine(_createAnImage.BasePath, name)
			};
			Directory.CreateDirectory(appSettings.DependenciesFolder);
			Directory.CreateDirectory(Path.Combine(appSettings.DependenciesFolder,
				"exiftool-unix"));
			var stream = StringToStreamHelper.StringToStream("#!/bin/bash");
			try
			{
				await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(
					stream,
					Path.Combine(appSettings.DependenciesFolder, "exiftool-unix", "exiftool"));
			}
			catch ( ArgumentOutOfRangeException e )
			{
				Console.WriteLine(e);
				throw;
			}

			return appSettings;
		}

		private void RemoveTempFolderWithExifTool(
			string name = "test temp")
		{
			var path = Path.Combine(_createAnImage.BasePath, name);
			if ( _hostFileSystem.ExistFolder(path) )
			{
				_hostFileSystem.FolderDelete(path);
			}
		}

		[TestMethod]
		public async Task RunChmodOnExifToolUnixExe_TempFolderWithSpace_UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}

			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/exiftool-11.99.zip",
					new ByteArrayContent(CreateAnExifToolWindows.Bytes.ToArray())
				},
				{
					"https://exiftool.org/Image-ExifTool-11.99.tar.gz",
					new ByteArrayContent(CreateAnExifToolTarGz.Bytes.ToArray())
				},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			var appSettings = await CreateTempFolderWithExifTool();

			Console.WriteLine(appSettings.DependenciesFolder);
			var exifToolDownload =
				new ExifToolDownload(httpClientHelper, appSettings, new FakeIWebLogger());
			var result = await exifToolDownload.RunChmodOnExifToolUnixExe();
			Assert.IsTrue(result);

			var lsLah = await Command.Run("ls", "-lah",
				Path.Combine(appSettings.DependenciesFolder, "exiftool-unix", "exiftool")).Task;

			Console.WriteLine(lsLah.StandardOutput);

			RemoveTempFolderWithExifTool();
			Assert.IsTrue(lsLah.StandardOutput.StartsWith("-rwxr-xr-x"));
		}

		[TestMethod]
		public async Task RunChmodOnExifToolUnixExe_Chmod644_UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Console.WriteLine("This test is for unix only");
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}

			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/exiftool-11.99.zip",
					new ByteArrayContent(CreateAnExifToolWindows.Bytes.ToArray())
				},
				{
					"https://exiftool.org/Image-ExifTool-11.99.tar.gz",
					new ByteArrayContent(CreateAnExifToolTarGz.Bytes.ToArray())
				},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			await CreateTempFolderWithExifTool("starsky-tmp-dependencies-4835793");

			// make NOT executable 644 
			var cmdResult = await Command.Run("chmod", "644", // not executable!
				Path.Combine(_exifToolUnixTempFolderPath, "exiftool")).Task;

			Assert.AreEqual(cmdResult.StandardError, string.Empty);
			var exifToolDownload =
				new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger());
			var result = await exifToolDownload.RunChmodOnExifToolUnixExe();

			RemoveTempFolderWithExifTool("starsky-tmp-dependencies-4835793");

			Assert.IsTrue(result);
		}

		[TestMethod]
		public async Task DownloadExifTool_Windows()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(),
				_serviceScopeFactory, new FakeIWebLogger());

			var result = await new ExifToolDownload(httpClientHelper, _appSettings,
				new FakeIWebLogger()).DownloadExifTool(true);

			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task DownloadExifTool_Skip_AddSwaggerExportExitAfter()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(),
				_serviceScopeFactory, new FakeIWebLogger());

			var appSettings = new AppSettings
			{
				AddSwaggerExport = true, AddSwaggerExportExitAfter = true
			};
			var logger = new FakeIWebLogger();
			var result = await new ExifToolDownload(httpClientHelper, appSettings, logger)
				.DownloadExifTool(true);
			Assert.IsFalse(result);
			Assert.IsTrue(logger.TrackedInformation[0].Item2?.Contains(
				"Skipped due true of AddSwaggerExport " +
				"and AddSwaggerExportExitAfter setting"));
		}

		[TestMethod]
		public async Task DownloadExifTool_Skip_ExiftoolSkipDownloadOnStartup()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(),
				_serviceScopeFactory, new FakeIWebLogger());

			var appSettings = new AppSettings { ExiftoolSkipDownloadOnStartup = true, };
			var logger = new FakeIWebLogger();
			var result = await new ExifToolDownload(httpClientHelper, appSettings, logger)
				.DownloadExifTool(true);
			Assert.IsFalse(result);
			Assert.IsTrue(logger.TrackedInformation[0].Item2
				?.Contains("Skipped due true of ExiftoolSkipDownloadOnStartup setting"));
		}

		[TestMethod]
		public async Task DownloadExifTool_Unix()
		{
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(),
				_serviceScopeFactory, new FakeIWebLogger());
			Directory.Delete(_appSettings.DependenciesFolder, true);
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger())
					.DownloadExifTool(false);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task DownloadExifTool_Windows_existVerbose()
		{
			var appSettings = new AppSettings
			{
				DependenciesFolder = AppDomain.CurrentDomain.BaseDirectory, Verbose = true
			};
			Directory.CreateDirectory(appSettings.DependenciesFolder);
			Directory.CreateDirectory(Path.Combine(appSettings.DependenciesFolder,
				"exiftool-windows"));

			var debugString = "\n\necho \"Fake ExifTool\"\n\n\n\necho 'test'";
			var stream = StringToStreamHelper.StringToStream(
				"#!/bin/bash\n" + debugString + debugString
				+ debugString + debugString + debugString);
			await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(stream,
				Path.Combine(appSettings.DependenciesFolder, "exiftool-windows", "exiftool.exe"));

			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(),
				_serviceScopeFactory, new FakeIWebLogger());
			var logger = new FakeIWebLogger();

			var result = await new ExifToolDownload(httpClientHelper, appSettings, logger)
				.DownloadExifTool(true);

			Assert.IsTrue(result);
			Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2?
				.Contains("[DownloadExifTool] " + appSettings.DependenciesFolder));

			Directory.Delete(Path.Combine(appSettings.DependenciesFolder, "exiftool-windows"),
				true);
		}

		[TestMethod]
		public async Task DownloadExifTool_existVerbose_UnixOnly()
		{
			if ( _appSettings.IsWindows )
			{
				Console.WriteLine("This test is for unix only");
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}

			var appSettings = await CreateTempFolderWithExifTool("test32");
			appSettings.Verbose = true;
			var httpClientHelper = new HttpClientHelper(new FakeIHttpProvider(),
				_serviceScopeFactory, new FakeIWebLogger());
			var logger = new FakeIWebLogger();
			var result = await new ExifToolDownload(httpClientHelper, appSettings, logger)
				.DownloadExifTool(false, 3);

			Assert.IsTrue(result);
			Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2?
				.Contains("[DownloadExifTool] " + appSettings.DependenciesFolder));

			Directory.Delete(appSettings.DependenciesFolder, true);
		}


		[TestMethod]
		public async Task StartDownloadForWindows_2Times()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/exiftool-11.99_64.zip",
					new ByteArrayContent([.. CreateAnExifToolWindows.Bytes])
				},
				{
					"https://qdraw.nl/special/mirror/exiftool/exiftool-11.99_64.zip",
					new ByteArrayContent([.. CreateAnExifToolWindows.Bytes])
				}
			});

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger())
					.StartDownloadForWindows();
			
			Assert.IsTrue(result);

			// And run again
			// ByteArray content is Disposed afterward
			var fakeIHttpProvider2 = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/exiftool-11.99_64.zip",
					new ByteArrayContent([.. CreateAnExifToolWindows.Bytes])
				}
			});
			var httpClientHelper2 = new HttpClientHelper(fakeIHttpProvider2, _serviceScopeFactory,
				new FakeIWebLogger());
			var result2 =
				await new ExifToolDownload(httpClientHelper2, _appSettings, new FakeIWebLogger())
					.StartDownloadForWindows();
			Assert.IsTrue(result2);

			_hostFileSystem.FolderDelete(_exifToolWindowsTempFolderPath);
		}

		[TestMethod]
		public async Task StartDownloadForWindows_Fail()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
			});

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			var result = await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger())
				.StartDownloadForWindows();
			
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task StartDownloadForUnix_Fail()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
			});

			var logger = new FakeIWebLogger();
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			var result = await new ExifToolDownload(httpClientHelper, _appSettings, logger)
				.StartDownloadForUnix();
			
			Assert.IsFalse(result);
			Assert.AreEqual(2, logger.TrackedExceptions.Count(p => p.Item2?.Contains("file is not downloaded") == true));
		}

		[TestMethod]
		public async Task StartDownloadForUnix_2Times()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/Image-ExifTool-11.99.tar.gz",
					new ByteArrayContent(CreateAnExifToolTarGz.Bytes.ToArray())
				}
			});

			_appSettings.Verbose = true;

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, new FakeIWebLogger())
					.StartDownloadForUnix();
			Assert.IsTrue(result);

			// And run again
			// ByteArray content is Disposed afterward
			var fakeIHttpProvider2 = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/Image-ExifTool-11.99.tar.gz",
					new ByteArrayContent(CreateAnExifToolTarGz.Bytes.ToArray())
				}
			});
			var httpClientHelper2 = new HttpClientHelper(fakeIHttpProvider2, _serviceScopeFactory,
				new FakeIWebLogger());
			var result2 =
				await new ExifToolDownload(httpClientHelper2, _appSettings, new FakeIWebLogger())
					.StartDownloadForUnix();
			Assert.IsTrue(result2);

			_hostFileSystem.FolderDelete(_exifToolUnixTempFolderPath);
		}

		[TestMethod]
		public void CheckSha256_Good()
		{
			var fakeIStorage = new FakeIStorage(new List<string> { "/" },
				["/exiftool.exe"],
				new List<byte[]> { CreateAnExifToolTarGz.Bytes.ToArray() });

			var result2 =
				new ExifToolDownload(null!, _appSettings, new FakeIWebLogger(), fakeIStorage)
					.CheckSha256("/exiftool.exe", new List<string> { CreateAnExifToolTarGz.Sha256 });
			Assert.IsTrue(result2);
		}

		[TestMethod]
		public void CheckSha1_Bad()
		{
			var fakeIStorage = new FakeIStorage(new List<string> { "/" },
				["/exiftool.exe"],
				new List<byte[]> { CreateAnExifToolTarGz.Bytes.ToArray() });

			var result2 =
				new ExifToolDownload(null!, _appSettings, new FakeIWebLogger(), fakeIStorage)
					.CheckSha256("/exiftool.exe", new List<string> { "random_value" });
			Assert.IsFalse(result2);
		}

		[TestMethod]
		public async Task StartDownloadForUnix_WrongHash()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{
					"https://exiftool.org/Image-ExifTool-11.99.tar.gz",
					new StringContent("FAIL")
				}
			});
			var logger = new FakeIWebLogger();
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, logger)
					.StartDownloadForUnix();
			
			Assert.IsFalse(result);
			Assert.AreEqual(1, logger.TrackedExceptions.Count(p => p.Item2?.Contains("file is not downloaded") == true));
		}

		[TestMethod]
		public async Task StartDownloadForWindows_WrongHash()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ "https://exiftool.org/checksums.txt", new StringContent(ExampleCheckSum) },
				{ "https://exiftool.org/exiftool-11.99_64.zip", new StringContent("FAIL") }
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			var logger = new FakeIWebLogger();
			var result =
				await new ExifToolDownload(httpClientHelper, _appSettings, logger)
					.StartDownloadForWindows();
			Assert.IsFalse(result);
			Assert.IsTrue(logger.TrackedExceptions.Exists(p => p.Item2?.Contains("Checksum for ") == true));
		}

		[TestMethod]
		public async Task DownloadForUnix_FromMirrorInsteadOfMainSource()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{
					"https://qdraw.nl/special/mirror/exiftool/exiftool-11.99.zip",
					new StringContent("FAIL")
				},
				{
					"https://qdraw.nl/special/mirror/exiftool/Image-ExifTool-11.99.tar.gz",
					new StringContent("FAIL")
				}
			});
			var logger = new FakeIWebLogger();
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				logger);

			await new ExifToolDownload(httpClientHelper, _appSettings, logger)
				.DownloadForUnix("Image-ExifTool-11.99.tar.gz",
					[]);

			Assert.IsTrue(logger.TrackedExceptions.Exists(p => p.Item2?.Contains("Checksum for ") == true));
			Assert.IsTrue(logger.TrackedExceptions.Exists(p => p.Item2?.Contains("is not valid") == true));
		}

		[TestMethod]
		public async Task DownloadForWindows_FromMirrorInsteadOfMainSource()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{
					"https://qdraw.nl/special/mirror/exiftool/exiftool-11.99_64.zip",
					new StringContent("FAIL")
				},
				{
					"https://qdraw.nl/special/mirror/exiftool/Image-ExifTool-11.99.tar.gz",
					new StringContent("FAIL")
				}
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var logger = new FakeIWebLogger();
			var result = await new ExifToolDownload(httpClientHelper, _appSettings, logger)
				.DownloadForWindows("exiftool-11.99_64.zip", []);
			
			Assert.IsFalse(result);
			Assert.AreEqual(1,logger.TrackedExceptions.Count(p => p.Item2?.Contains("Checksum for") == true));
		}

		[TestMethod]
		public void GetChecksumsFromTextFile_ToManyShaResults()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>());

			const string example = "SHA256(Image-ExifTool-12.94.tar.gz)= d029485b7aff73e1c4806bbaaf87617dd98c5d2762f1d3a033e0ca926d7484e0\n" +
			                       "SHA256(exiftool-12.94_32.zip)= e0521db2115b3ee07f531ed7e3f686c57fca23b742c8f88b387aef6b682a12fe\n" +
			                       "SHA256(exiftool-12.94_64.zip)= 60df34ccc3440e18738304fe294c38b127eb8a88c2316abc1dbd7f634a23ee7a\n" +
			                       "SHA256(ExifTool-12.94.pkg)= 59f96cf7c5250b609ad1c8e56cd7ddccc620b5448a3d8b3f3368f39a580a1059\n" +
			                       "MD5 (Image-ExifTool-12.40.tar.gz) = 72b40d69cf518edebbf5b661465950e7\n" +
			                       "MD5 (exiftool-12.40.zip) = fc834fd43d79da19fcb6461fb791b275\n" +
			                       "MD5 (ExifTool-12.40.dmg) = b30e391a4b53564de60a72f4347cade4\n";

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var exifToolDownload = new ExifToolDownload(httpClientHelper, new AppSettings(),
				new FakeIWebLogger(), new FakeIStorage());
			var result = exifToolDownload.GetChecksumsFromTextFile(example, 2);
			Assert.AreEqual(0, result.Length);
		}


		[TestMethod]
		public void GetChecksumsFromTextFile_Good()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>());

			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var exifToolDownload = new ExifToolDownload(httpClientHelper, new AppSettings(),
				new FakeIWebLogger(), new FakeIStorage());
			var result = exifToolDownload.GetChecksumsFromTextFile(ExampleCheckSum);
			Assert.AreEqual(4, result.Length);
		}

		[DataTestMethod] // [Theory]
		[DataRow(true)]
		[DataRow(false)]
		public void MoveFileIfExist_WithFolder(bool outputFolderAlreadyExists)
		{
			var storage = new FakeIStorage();
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>());
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());
			var exifToolDownload = new ExifToolDownload(httpClientHelper, new AppSettings(),
				new FakeIWebLogger(), storage);

			storage.CreateDirectory("/");
			storage.CreateDirectory("/test");
			storage.CreateDirectory($"/test/test");
			storage.CreateDirectory($"/test/test/test_01");
			if ( outputFolderAlreadyExists )
			{
				storage.CreateDirectory("/output");
			}
			
			exifToolDownload.MoveFileIfExist("/", "test_01", "/", "output" );

			Assert.IsTrue(storage.ExistFolder("/output"));
		}
	}
}
