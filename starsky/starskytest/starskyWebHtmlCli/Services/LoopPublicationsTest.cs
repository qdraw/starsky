using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
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
            var createAnImage = new CreateAnImage();
            
            appSettings.PublishProfiles.Add(new AppSettingsPublishProfiles
            {
                ContentType = TemplateContentType.Jpeg,
                Path = createAnImage.FullFilePath, 
                // Folder = ""
            });

            // Add large image
            appSettings.PublishProfiles.Add(new AppSettingsPublishProfiles
            {
                ContentType = TemplateContentType.Jpeg,
                Path = createAnImage.FullFilePath, 
                SourceMaxWidth = 1001
            });
            
            

            // Move to the same folder
            appSettings.PublishProfiles.Add(new AppSettingsPublishProfiles
            {
                ContentType = TemplateContentType.MoveSourceFiles,
            });

            appSettings.StorageFolder = createAnImage.BasePath;
            appSettings.ThumbnailTempFolder = createAnImage.BasePath;
            var list = new List<FileIndexItem> {new FileIndexItem
            {
                FileName = createAnImage.FileName,
                FileHash = createAnImage.FileName.Replace(".jpg",string.Empty)
            }};

            new LoopPublications(new FakeIStorage(), appSettings,null).Render(list,null);
            
            FilesHelper.DeleteFile(createAnImage.BasePath + Path.DirectorySeparatorChar + "index.html");
        }
    }
}
