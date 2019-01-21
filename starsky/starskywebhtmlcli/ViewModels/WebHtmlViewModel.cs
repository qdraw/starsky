﻿using System.Collections.Generic;
using starskycore.Models;

namespace starskywebhtmlcli.ViewModels
{
    public class WebHtmlViewModel
    {
        public AppSettings AppSettings { get; set; }
        public AppSettingsPublishProfiles Profile { get; set; }
        public string[] Base64ImageArray { get; set; }
        public List<FileIndexItem> FileIndexItems { get; set; }
    }
}