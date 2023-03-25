using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.http.Interfaces;
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
	    private readonly string _dependenciesFolder;

	    public GeoFileDownloadTests()
	    {
		    _dependenciesFolder = Path.Combine(new CreateAnImage().BasePath, "GeoFileDownloadTests-deps");
	    }

        [TestCleanup]
        public void ClassCleanup()
		{
			new StorageHostFullPathFilesystem().FolderDelete(_dependenciesFolder);
		}
        
        [TestMethod]
        public async Task DownloadAsync_ShouldDownloadFileIfNotExists()
        {
            // Arrange
            var storage = new StorageHostFullPathFilesystem();
            storage.CreateDirectory(_dependenciesFolder);
            
            var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder };
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
                Path.Combine(_dependenciesFolder, GeoFileDownload.CountryName + ".zip")));

            Assert.IsTrue(new StorageHostFullPathFilesystem().ExistFile(
                Path.Combine(_dependenciesFolder, "admin1CodesASCII.txt")));
            
            storage.FolderDelete(_dependenciesFolder);
        }
        
        [TestMethod]
        public async Task DownloadAsync_ShouldDownloadFileIfNotExists_DownloadFromMirror()
        {
	        // Arrange
	        var storage = new StorageHostFullPathFilesystem();
	        storage.CreateDirectory(_dependenciesFolder);
            
	        var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder };
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
		        Path.Combine(_dependenciesFolder, GeoFileDownload.CountryName + ".zip")));

	        Assert.IsTrue(new StorageHostFullPathFilesystem().ExistFile(
		        Path.Combine(_dependenciesFolder, "admin1CodesASCII.txt")));
            
	        storage.FolderDelete(_dependenciesFolder);
        }

        [TestMethod]
        public async Task DownloadAsync_ShouldNotDownloadFileIfAlreadyExists()
        {
            // Arrange
            var storage = new StorageHostFullPathFilesystem();
            storage.CreateDirectory(_dependenciesFolder);
            
            var httpClientHelper = new FakeIHttpClientHelper(storage, new Dictionary<string, KeyValuePair<bool, string>>());
            
            var appSettings = new AppSettings { DependenciesFolder = _dependenciesFolder };
            var geoFileDownload = new GeoFileDownload(appSettings, httpClientHelper);
        
            storage.CreateDirectory(_dependenciesFolder);
            await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("1"),Path.Combine(_dependenciesFolder, GeoFileDownload.CountryName + ".txt"));
            await storage.WriteStreamAsync(PlainTextFileHelper.StringToStream("1"), Path.Combine(_dependenciesFolder, "admin1CodesASCII.txt"));
        
            // Act
            await geoFileDownload.DownloadAsync();
        
            // Assert
            Assert.IsTrue(!httpClientHelper.UrlsCalled.Any());
            
            storage.FolderDelete(_dependenciesFolder);
        }
    }
}
