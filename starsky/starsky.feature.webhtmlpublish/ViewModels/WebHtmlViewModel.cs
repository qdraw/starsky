using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.ViewModels
{
    public class WebHtmlViewModel
    {
	    /// <summary>
	    /// Used with helpers
	    /// </summary>
        public AppSettings AppSettings { get; set; }
        
        /// <summary>
        /// Current profile
        /// </summary>
        public AppSettingsPublishProfiles CurrentProfile { get; set; }
        
        /// <summary>
        /// Other profiles within the same group
        /// </summary>
        public List<AppSettingsPublishProfiles> Profiles { get; set; }

        public string[] Base64ImageArray { get; set; }
        public List<FileIndexItem> FileIndexItems { get; set; }
    }
}
