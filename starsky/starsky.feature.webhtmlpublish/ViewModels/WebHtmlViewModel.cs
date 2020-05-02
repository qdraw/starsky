﻿using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.ViewModels
{
    public class WebHtmlViewModel
    {
        public AppSettings AppSettings { get; set; }
        public AppSettingsPublishProfiles Profile { get; set; }
        public string[] Base64ImageArray { get; set; }
        public List<FileIndexItem> FileIndexItems { get; set; }
    }
}
