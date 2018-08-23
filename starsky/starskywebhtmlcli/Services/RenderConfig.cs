using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starskywebhtmlcli.Services
{
    public class RenderConfig
    {
        private readonly AppSettings _appSettings;

        public RenderConfig(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        
        public void Render(List<FileIndexItem> fileIndexItemsList)
        {
            if(!_appSettings.PublishProfiles.Any()) Console.WriteLine("There are no config items");
            
            foreach (var profile in _appSettings.PublishProfiles)
            {
                if (profile.ContentType == TemplateContentType.Html)
                {
                    new ViewRender(_appSettings).RazorRender(fileIndexItemsList,profile.Template,profile.Path);
                }
                
                if (profile.ContentType == TemplateContentType.Jpeg)
                {
                    foreach (var item in fileIndexItemsList)
                    {
                        new OverlayImage(_appSettings).ResizeOverlayImage(
                            _appSettings.DatabasePathToFilePath(item.FilePath), profile);
                    }
                }

                

            }
        }
    }
}