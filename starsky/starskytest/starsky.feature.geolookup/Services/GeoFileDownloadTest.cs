using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
    [TestClass]
    public class GeoFileDownloadTests
    {
	    private readonly string _dependenciesFolder1;
	    private readonly string _dependenciesFolder2;
	    private readonly string _dependenciesFolder3;
	    private readonly string _dependenciesFolder4;
	    private readonly string _dependenciesFolder5;
	    private readonly string _dependenciesFolder6;

	    public GeoFileDownloadTests()
	    {
		    _dependenciesFolder1 = Path.Combine(new CreateAnImage().BasePath, "GeoFileDownloadTests-deps_01");
		    _dependenciesFolder2 = Path.Combine(new CreateAnImage().BasePath, "GeoFileDownloadTests-deps_02");
		    _dependenciesFolder3 = Path.Combine(new CreateAnImage().BasePath, "GeoFileDownloadTests-deps_03");
		    _dependenciesFolder4 = Path.Combine(new CreateAnImage().BasePath, "GeoFileDownloadTests-deps_04");
		    _dependenciesFolder5 = Path.Combine(new CreateAnImage().BasePath, "GeoFileDownloadTests-deps_05");
		    _dependenciesFolder6 = Path.Combine(new CreateAnImage().BasePath, "GeoFileDownloadTests-deps_06");
	    }

        [TestCleanup]
        public void ClassCleanup()
		{
			new StorageHostFullPathFilesystem().FolderDelete(_dependenciesFolder1);
			new StorageHostFullPathFilesystem().FolderDelete(_dependenciesFolder2);
			new StorageHostFullPathFilesystem().FolderDelete(_dependenciesFolder3);
			new StorageHostFullPathFilesystem().FolderDelete(_dependenciesFolder4);
			new StorageHostFullPathFilesystem().FolderDelete(_dependenciesFolder5);
			new StorageHostFullPathFilesystem().FolderDelete(_dependenciesFolder6);
		}
        
        [TestMethod]
        public async Task DownloadAsync_ShouldDownloadFileIfNotExists()
        {
            // Arrange
            var storage = new StorageHostFullPathFilesystem();
            storage.CreateDirectory(_dependenciesFolder1);
            
            var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder1 };
            var httpClientHelper = new FakeIHttpClientHelper(storage, new Dictionary<string, KeyValuePair<bool, string>>
            {
	            {
		            $"https://{GeoFileDownload.BaseUrl}{GeoFileDownload.CountryName}.zip", new KeyValuePair<bool, string>(true, "UEsFBgAAAAAAAAAAAAAAAAAAAAAAAA==")
	            },
	            {
		            $"https://{GeoFileDownload.BaseUrl}admin1CodesASCII.txt", new KeyValuePair<bool, string>(true, "UEsFBgAAAAAAAAAAAAAAAAAAAAAAAA==")
	            }
            });
            
            var geoFileDownload = new GeoFileDownload(appSettings, httpClientHelper);

            // Act
            await geoFileDownload.DownloadAsync();

            // Assert
            Assert.IsTrue(new StorageHostFullPathFilesystem().ExistFile(
                Path.Combine(_dependenciesFolder1, GeoFileDownload.CountryName + ".zip")));

            Assert.IsTrue(new StorageHostFullPathFilesystem().ExistFile(
                Path.Combine(_dependenciesFolder1, "admin1CodesASCII.txt")));
            
            storage.FolderDelete(_dependenciesFolder1);
        }
        
        [TestMethod]
        public async Task DownloadAsync_ShouldDownloadFileIfNotExists_DownloadFromMirror()
        {
	        // Arrange
	        var storage = new StorageHostFullPathFilesystem();
	        storage.CreateDirectory(_dependenciesFolder2);
            
	        var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder2 };
	        var httpClientHelper = new FakeIHttpClientHelper(storage, new Dictionary<string, KeyValuePair<bool, string>>
	        {
		        {
			        $"https://{GeoFileDownload.MirrorUrl}{GeoFileDownload.CountryName}.zip", new KeyValuePair<bool, string>(true, "UEsFBgAAAAAAAAAAAAAAAAAAAAAAAA==")
		        },
		        {
			        $"https://{GeoFileDownload.MirrorUrl}admin1CodesASCII.txt", new KeyValuePair<bool, string>(true, "UEsFBgAAAAAAAAAAAAAAAAAAAAAAAA==")
		        }
	        });
            
	        var geoFileDownload = new GeoFileDownload(appSettings, httpClientHelper);

	        // Act
	        await geoFileDownload.DownloadAsync();

	        // Assert
	        Assert.IsTrue(new StorageHostFullPathFilesystem().ExistFile(
		        Path.Combine(_dependenciesFolder2, GeoFileDownload.CountryName + ".zip")));

	        Assert.IsTrue(new StorageHostFullPathFilesystem().ExistFile(
		        Path.Combine(_dependenciesFolder2, "admin1CodesASCII.txt")));
            
	        storage.FolderDelete(_dependenciesFolder2);
        }

        [TestMethod]
        public async Task DownloadAsync_ShouldNotDownloadFileIfAlreadyExists()
        {
            // Arrange
            var storage = new StorageHostFullPathFilesystem();
            storage.CreateDirectory(_dependenciesFolder3);
            
            var httpClientHelper = new FakeIHttpClientHelper(storage, new Dictionary<string, KeyValuePair<bool, string>>());
            
            var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder3 };
            var geoFileDownload = new GeoFileDownload(appSettings, httpClientHelper);
        
            storage.CreateDirectory(_dependenciesFolder3);
            await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("1"),Path.Combine(_dependenciesFolder3, GeoFileDownload.CountryName + ".txt"));
            await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("1"), Path.Combine(_dependenciesFolder3, "admin1CodesASCII.txt"));
        
            // Act
            await geoFileDownload.DownloadAsync();
        
            // Assert
            Assert.IsTrue(!httpClientHelper.UrlsCalled.Any());
            
            storage.FolderDelete(_dependenciesFolder3);
        }

        [TestMethod]
        public void CreateDependenciesFolder_ShouldBeCreated()
        {
	        var storage = new StorageHostFullPathFilesystem();
	        var httpClientHelper = new FakeIHttpClientHelper(storage, new Dictionary<string, KeyValuePair<bool, string>>());
	        var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder4 };
	        var geoFileDownload = new GeoFileDownload(appSettings, httpClientHelper);
	        geoFileDownload.CreateDependenciesFolder();
	        
	        Assert.IsTrue(storage.ExistFolder(_dependenciesFolder4));
	        
	        storage.FolderDelete(_dependenciesFolder4);
        }

        [TestMethod]
        public async Task RemoveFailedDownload_FileToSmall_SoRemove()
        {
	        var storage = new StorageHostFullPathFilesystem();
	        var httpClientHelper = new FakeIHttpClientHelper(storage, new Dictionary<string, KeyValuePair<bool, string>>());
	        var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder5 };
	        var geoFileDownload = new GeoFileDownload(appSettings, httpClientHelper);
	        
	        geoFileDownload.CreateDependenciesFolder();

	        await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("1"),Path.Combine(_dependenciesFolder5, GeoFileDownload.CountryName + ".zip"));
	        Assert.IsTrue(storage.ExistFile(Path.Combine(_dependenciesFolder5, GeoFileDownload.CountryName + ".zip")));

	        geoFileDownload.RemoveFailedDownload();
	        
	        Assert.IsTrue(storage.ExistFolder(_dependenciesFolder5));
	        Assert.IsFalse(storage.ExistFile(Path.Combine(_dependenciesFolder5, GeoFileDownload.CountryName + ".zip")));
        }
        
        [TestMethod]
        public async Task RemoveFailedDownload_FileRightSize_SoKeep()
        {
	        var storage = new StorageHostFullPathFilesystem();
	        var httpClientHelper = new FakeIHttpClientHelper(storage, new Dictionary<string, KeyValuePair<bool, string>>());
	        var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder6 };
	        var geoFileDownload = new GeoFileDownload(appSettings, httpClientHelper);
	        
	        geoFileDownload.CreateDependenciesFolder();
	        geoFileDownload.MinimumSizeInBytes = -1;
	        
	        await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("1"),Path.Combine(_dependenciesFolder6, GeoFileDownload.CountryName + ".zip"));
	        Assert.IsTrue(storage.ExistFile(Path.Combine(_dependenciesFolder6, GeoFileDownload.CountryName + ".zip")));

	        geoFileDownload.RemoveFailedDownload();
	        
	        Assert.IsTrue(storage.ExistFolder(_dependenciesFolder6));
	        Assert.IsTrue(storage.ExistFile(Path.Combine(_dependenciesFolder6, GeoFileDownload.CountryName + ".zip")));
        }
    }
}
