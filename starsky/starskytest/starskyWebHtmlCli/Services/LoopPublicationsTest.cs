using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;
using starskywebhtmlcli.Services;

namespace starskytest.starskyWebHtmlCli.Services
{
	[TestClass]
    public class LoopPublicationsTest
    {
        [TestMethod]
        public void LoopPublicationsTestRunRenderVoid()
        {
            var appSettings = new AppSettings
            {
                PublishProfiles = new List<AppSettingsPublishProfiles>
                {
                    new AppSettingsPublishProfiles
                    {
                        ContentType = TemplateContentType.Html,
                        Path = "index.html",
                        Template = "Index.cshtml"
                    }
                }
            };
            
            appSettings.PublishProfiles.Add(new AppSettingsPublishProfiles
            {
                ContentType = TemplateContentType.Jpeg,
                Path = new CreateAnImage().FullFilePath,  // <== overlay image; depends on fs
	            SourceMaxWidth = 150
            });

            // Add large image
            appSettings.PublishProfiles.Add(new AppSettingsPublishProfiles
            {
                ContentType = TemplateContentType.Jpeg,
                Path = new CreateAnImage().FullFilePath, // <== overlay image; depends on fs
                SourceMaxWidth = 1200
            });

            // Move to the same folder
            appSettings.PublishProfiles.Add(new AppSettingsPublishProfiles
            {
                ContentType = TemplateContentType.MoveSourceFiles,
            });

	        
            var list = new List<FileIndexItem> {new FileIndexItem
            {
                FileName = "/test.jpg",
                FileHash = "FILEHASH"
            }};
            
	        var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
		        new List<string>{"/test.jpg","FILEHASH"},new List<byte[]>{CreateAnImage.Bytes, CreateAnImage.Bytes,});
	        var selectorStorage = new FakeSelectorStorage(fakeStorage);
	        
	        var template = "<html></html>";
	        fakeStorage.WriteStream(new PlainTextFileHelper().StringToStream(template),
		        new EmbeddedViewsPath().GetViewFullPath("Index.cshtml"));
	        
            new LoopPublications(selectorStorage, appSettings,
	            new FakeExifTool(fakeStorage,appSettings), new ReadMeta(fakeStorage)).Render(list,null);

	        var dir = fakeStorage.GetAllFilesInDirectory("/").ToList();

        }
    }
}
