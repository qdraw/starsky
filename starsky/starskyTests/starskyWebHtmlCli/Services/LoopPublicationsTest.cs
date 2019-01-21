using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Models;
using starskycore.Helpers;
using starskycore.Models;
using starskytests.FakeCreateAn;
using starskywebhtmlcli.Services;

namespace starskytests.starskyWebHtmlCli.Services
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

            new LoopPublications(appSettings,null).Render(list,null);
            
            Files.DeleteFile(createAnImage.BasePath + Path.DirectorySeparatorChar + "index.html");
        }
    }
}
