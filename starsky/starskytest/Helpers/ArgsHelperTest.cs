using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Attributes;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskytest.FakeCreateAn;

namespace starskytest.Helpers
{
    [TestClass]
    public class ArgsHelperTest
    {
        private readonly AppSettings _appSettings;

        public ArgsHelperTest()
        {
            // Add a dependency injection feature
            var services = new ServiceCollection();
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // Make example config in memory
            var newImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", newImage.BasePath },
                { "App:Verbose", "true" }
            };
            // Start using dependency injection
            var builder = new ConfigurationBuilder();  
            // Add random config to dependency injection
            builder.AddInMemoryCollection(dict);
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            // get the service
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_NeedVerboseTest()
        {
            var args = new List<string> {"-v"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).NeedVerbose(args), true);
            
            // Bool parse check
            args = new List<string> {"-v","true"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).NeedVerbose(args), true);
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_NeedRecruisiveTest()
        {
            var args = new List<string> {"-r"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).NeedRecruisive(args), true);
            
            // Bool parse check
            args = new List<string> {"-r","true"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).NeedRecruisive(args), true);
        }
	    
	    [TestMethod]
	    [ExcludeFromCoverage]
	    public void ArgsHelper_NeedCacheCleanupTest()
	    {
		    var args = new List<string> {"-x"}.ToArray();
		    Assert.AreEqual(new ArgsHelper(_appSettings).NeedCacheCleanup(args), true);
            
		    // Bool parse check
		    args = new List<string> {"-x","true"}.ToArray();
		    Assert.AreEqual(new ArgsHelper(_appSettings).NeedCacheCleanup(args), true);
	    }
	    
	    

        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetIndexModeTest()
        {
            // Default on so testing off
            var args = new List<string> {"-i","false"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).GetIndexMode(args), false);
        }
        
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_NeedHelpTest()
        {
            var args = new List<string> {"-h"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).NeedHelp(args), true);

			// Bool parse cheArgsHelper_GetPath_CurrentDirectory_Testck
			args = new List<string> {"-h","true"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).NeedHelp(args), true);
        }
        
        [TestMethod]
        public void ArgsHelper_GetPathFormArgsTest()
        {
            var args = new List<string> {"-p", "/"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).GetPathFormArgs(args), "/");
        }

	    [TestMethod]
	    [ExpectedException(typeof(ArgumentNullException))]
	    public void ArgsHelper_GetPathFormArgsTest_ArgumentNullException()
	    {
		    // inject appsettings!
		    var args = new List<string> {"-p", "/"}.ToArray();
		    new ArgsHelper(null).GetPathFormArgs(args);
	    }

	    [TestMethod]
	    public void ArgsHelper_GetPath_WithHelp_CurrentDirectory_Test()
	    {
		    var args = new List<string> { "-p", "-h" }.ToArray();
		    var value = new ArgsHelper(_appSettings).GetPathFormArgs(args,false);

		    var currentDir = Directory.GetCurrentDirectory();
			Assert.AreEqual(currentDir, value);
	    }

	    [TestMethod]
	    public void ArgsHelper_GetPath_CurrentDirectory_Test()
	    {
		    var args = new List<string> { "-p" }.ToArray();
		    var value = new ArgsHelper(_appSettings).GetPathFormArgs(args,false);

		    Assert.AreEqual(Directory.GetCurrentDirectory(), value);
	    }

	    
		[TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetSubpathFormArgsTest()
        {
            _appSettings.StorageFolder = new CreateAnImage().BasePath;
            var args = new List<string> {"-s", "/"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).GetSubpathFormArgs(args), "/");
        }    
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_IfSubpathTest()
        {
            _appSettings.StorageFolder = new CreateAnImage().BasePath;
            var args = new List<string> {"-s", "/"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).IfSubpathOrPath(args), true);
            
            // Default
            args = new List<string>{string.Empty}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).IfSubpathOrPath(args), true);
            
            args = new List<string> {"-p", "/"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).IfSubpathOrPath(args), false);
        }

	    [TestMethod]
	    public void ArgsHelper_CurrentDirectory_IfSubpathTest()
	    {
		    // for selecting the current directory
		    var args = new List<string> {"-p"}.ToArray();
		    Assert.AreEqual(new ArgsHelper(_appSettings).IfSubpathOrPath(args), false);
	    }

	    [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetThumbnailTest()
        {
            _appSettings.StorageFolder = new CreateAnImage().BasePath;
            var args = new List<string> {"-t", "true"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).GetThumbnail(args), true);
        }   
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetOrphanFolderCheckTest()
        {
            _appSettings.StorageFolder = new CreateAnImage().BasePath;
            var args = new List<string> {"-o", "true"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).GetOrphanFolderCheck(args), true);
        }   
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetMoveTest()
        {
            var args = new List<string> {"-m"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).GetMove(args), true);
            
            // Bool parse check
            args = new List<string> {"-m","true"}.ToArray();
            Assert.AreEqual(new ArgsHelper(_appSettings).GetMove(args), true);
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetAllTest()
        {
            var args = new List<string> {"-a"}.ToArray();
            Assert.AreEqual(true, new ArgsHelper(_appSettings).GetAll(args));
            
            // Bool parse check
            args = new List<string> {"-a","false"}.ToArray();
            Assert.AreEqual(false, new ArgsHelper(_appSettings).GetAll(args));
            
            args = new List<string> {}.ToArray();
            Assert.AreEqual(false, new ArgsHelper(_appSettings).GetAll(args));
            
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_SetEnvironmentByArgsShortTestListTest()
        {
            var shortNameList = new ArgsHelper(_appSettings).ShortNameList.ToArray();
            var envNameList = new ArgsHelper(_appSettings).EnvNameList.ToArray();

            var shortTestList = new List<string>();
            for (int i = 0; i < shortNameList.Length; i++)
            {
                shortTestList.Add(shortNameList[i]);
                shortTestList.Add(i.ToString());
            }
            
            new ArgsHelper(_appSettings).SetEnvironmentByArgs(shortTestList);
            
            for (int i = 0; i < envNameList.Length; i++)
            {
                Assert.AreEqual(Environment.GetEnvironmentVariable(envNameList[i]),i.ToString());
            }
            
            // Reset Environment after use
            foreach (var t in envNameList)
            {
                Environment.SetEnvironmentVariable(t,string.Empty);
            }
            
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_SetEnvironmentByArgsLongTestListTest()
        {
            var longNameList = new ArgsHelper(_appSettings).LongNameList.ToArray();
            var envNameList = new ArgsHelper(_appSettings).EnvNameList.ToArray();
            
            var longTestList = new List<string>();
            for (int i = 0; i < longNameList.Length; i++)
            {
                longTestList.Add(longNameList[i]);
                longTestList.Add(i.ToString());
            }
            
            new ArgsHelper(_appSettings).SetEnvironmentByArgs(longTestList);

            for (int i = 0; i < envNameList.Length; i++)
            {
                Assert.AreEqual(Environment.GetEnvironmentVariable(envNameList[i]),i.ToString());
            }
            
            // Reset Environment after use
            foreach (var t in envNameList)
            {
                Environment.SetEnvironmentVariable(t,string.Empty);
            }
            
        }

        [TestMethod]
        public void ArgsHelper_GetSubpathRelativeTest()
        {
            var args = new List<string> {"--subpathrelative", "1"}.ToArray();
            var relative = new ArgsHelper(_appSettings).GetSubpathRelative(args);
            var yesterdayString = DateTime.Today.AddDays(-1).ToString("yyyy_MM_dd");
            Assert.AreEqual(true, relative.Contains(yesterdayString));
        }

        [TestMethod]
        public void ArgsHelper_NeedHelpShowDialog()
        {
                // Just simple show a console dialog
            new ArgsHelper(new AppSettings {ApplicationType = AppSettings.StarskyAppType.WebHtml})
                .NeedHelpShowDialog();
            new ArgsHelper(new AppSettings {ApplicationType = AppSettings.StarskyAppType.Importer})
                .NeedHelpShowDialog();
			new ArgsHelper(new AppSettings {ApplicationType = AppSettings.StarskyAppType.Geo})
				.NeedHelpShowDialog();
        }

	    [TestMethod]
	    public void ArgsHelper_SetEnvironmentToAppSettingsTest()
	    {
		    var appSettings = new AppSettings();
		    
		    
		    var shortNameList = new ArgsHelper(appSettings).ShortNameList.ToArray();
		    var envNameList = new ArgsHelper(appSettings).EnvNameList.ToArray();

		    var shortTestList = new List<string>();
		    for (int i = 0; i < envNameList.Length; i++)
		    {
			    shortTestList.Add(shortNameList[i]);

			    if ( envNameList[i] == "app__DatabaseType" )
			    {
				    shortTestList.Add("InMemoryDatabase"); // need to exact good
				    continue; 
			    }

			    if ( envNameList[i] == "app__Structure" )
			    {
				    shortTestList.Add("/{filename}.ext");
				    continue; 
			    }
			    
			    if ( envNameList[i] == "app__DatabaseConnection" )
			    {
				    shortTestList.Add("test");
				    continue; 
			    }
			    
			    if ( envNameList[i] == "app__ExifToolPath" )
			    {
				    shortTestList.Add("app__ExifToolPath");
				    continue; 
			    }
			    if ( envNameList[i] == "app__StorageFolder" )
			    {
				    shortTestList.Add("app__StorageFolder");
				    continue; 
			    }
			    
			    shortTestList.Add(i.ToString());
		    }
            
		    // First inject values to evn
		    new ArgsHelper(appSettings).SetEnvironmentByArgs(shortTestList);
		    
		    
		    // and now read it back
		    new ArgsHelper(appSettings).SetEnvironmentToAppSettings();


		    Assert.AreEqual("/{filename}.ext",appSettings.Structure);
		    Assert.AreEqual(AppSettings.DatabaseTypeList.InMemoryDatabase,appSettings.DatabaseType);
		    Assert.AreEqual("test",appSettings.DatabaseConnection);
		    Assert.AreEqual("app__ExifToolPath",appSettings.ExifToolPath);
		    Assert.AreEqual(true,appSettings.StorageFolder.Contains("app__StorageFolder"));

		    
		    
		    // Reset Environment after use
		    foreach (var t in envNameList)
		    {
			    Environment.SetEnvironmentVariable(t,string.Empty);
		    }
	    }

    }
}
