﻿using System.Collections.Generic;
using starsky.Models;

namespace starskywebhtmlcli.ViewModels
{
    public class WebHtmlViewModel
    {
        public List<FileIndexItem> FileIndexItems { get; set; }
        public AppSettings AppSettings { get; set; }
        public AppSettingsPublishProfiles Profile { get; set; }
        public string[] Base64ImageArray { get; set; }
    }
}