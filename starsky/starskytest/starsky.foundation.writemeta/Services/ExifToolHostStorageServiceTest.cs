using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Services
{
	[TestClass]
	public sealed class ExifToolHostStorageServiceTest
	{
		private readonly CreateAnImage _createAnImage;

		public ExifToolHostStorageServiceTest()
		{
			_createAnImage = new CreateAnImage();
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task ExifToolHostStorageService_NotFound_Exception()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			await new ExifToolHostStorageService(new FakeSelectorStorage(fakeStorage), appSettings, new FakeIWebLogger())
				.WriteTagsAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
		}
		
		
		/// <summary>
		/// WriteTagsAndRenameThumbnailAsyncTest
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task WriteTagsAndRenameThumbnailAsync_NotFound_Exception()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			await new ExifToolHostStorageService(new FakeSelectorStorage(fakeStorage), appSettings, new FakeIWebLogger())
				.WriteTagsAndRenameThumbnailAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
		}
		
		[TestMethod]
		public async Task WriteTagsAndRenameThumbnailAsync_FakeExifToolBashTest_UnixOnly()
		{
			if ( new AppSettings().IsWindows )
			{
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}

			var hostFileSystemStorage = new StorageHostFullPathFilesystem();
			var memoryStream = new MemoryStream(CreateAnExifToolTarGz.Bytes);
			var outputPath =
				Path.Combine(_createAnImage.BasePath, "tmp-3426782387");
			if ( hostFileSystemStorage.ExistFolder(outputPath) )
			{
				hostFileSystemStorage.FolderDelete(outputPath);
			}
			new TarBal(hostFileSystemStorage).ExtractTarGz(memoryStream, outputPath);
			var imageExifToolVersionFolder = hostFileSystemStorage.GetDirectories(outputPath)
				.FirstOrDefault(p => p.StartsWith(Path.Combine(outputPath, "Image-ExifTool-")))?.Replace("./", string.Empty);

			if ( imageExifToolVersionFolder == null )
			{
				throw new FileNotFoundException("imageExifToolVersionFolder: "+ outputPath);
			}

			await Command.Run("chmod", "+x",
				Path.Combine(imageExifToolVersionFolder, "exiftool")).Task;
			
			var appSettings = new AppSettings
			{
				ExifToolPath = Path.Combine(imageExifToolVersionFolder, "exiftool"),
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes});

			var fakeLogger = new FakeIWebLogger();
			var renameThumbnailAsync = await new ExifToolHostStorageService(new FakeSelectorStorage(fakeStorage), appSettings, fakeLogger)
				.WriteTagsAndRenameThumbnailAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
			
			if ( hostFileSystemStorage.ExistFolder(outputPath) )
			{
				hostFileSystemStorage.FolderDelete(outputPath);
			}
			
			Assert.IsFalse(renameThumbnailAsync.Key);
			Assert.IsTrue(fakeLogger.TrackedExceptions.Any(p => p.Item2.Contains("Fake Exiftool detected")));
		}
		
		[TestMethod]
		public async Task WriteTagsAndRenameThumbnailAsync_FakeExifToolTest_WindowsOnly()
		{
			if ( !new AppSettings().IsWindows )
			{
				Assert.Inconclusive("This test if for Windows Only");
				return;
			}

			var hostFileSystemStorage = new StorageHostFullPathFilesystem();
			var outputPath =
				Path.Combine(_createAnImage.BasePath, "tmp-979056548");

			Console.WriteLine(outputPath);
			if ( hostFileSystemStorage.ExistFolder(outputPath) )
			{
				hostFileSystemStorage.FolderDelete(outputPath);
			}
			hostFileSystemStorage.CreateDirectory(outputPath);

			var result = Zipper.ExtractZip(CreateAnExifToolWindows.Bytes);
			var (_,item) = result.FirstOrDefault(p => p.Key.Contains("exiftool"));
			
			await hostFileSystemStorage.WriteStreamAsync(new MemoryStream(item), 
				Path.Combine(outputPath, "exiftool.exe"));
			
			var appSettings = new AppSettings
			{
				ExifToolPath = Path.Combine(outputPath, "exiftool.exe"),
			};
			
			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			var fakeLogger = new FakeIWebLogger();
			var renameThumbnailAsync = await new ExifToolHostStorageService(new FakeSelectorStorage(fakeStorage), appSettings, fakeLogger)
				.WriteTagsAndRenameThumbnailAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
			
			if ( hostFileSystemStorage.ExistFolder(outputPath) )
			{
				hostFileSystemStorage.FolderDelete(outputPath);
			}
			
			Assert.IsFalse(renameThumbnailAsync.Key);
			Assert.IsTrue(fakeLogger.TrackedExceptions.Any(p => p.Item2.Contains("Fake Exiftool detected")));
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task ExifToolHostStorageService_WriteTagsThumbnailAsync_NotFound_Exception()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			await new ExifToolHostStorageService(new FakeSelectorStorage(fakeStorage), appSettings, new FakeIWebLogger())
				.WriteTagsThumbnailAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
		}
	}
}
