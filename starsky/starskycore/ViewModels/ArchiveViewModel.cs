using System.Collections.Generic;
using starsky.ViewModels;
using starskycore.Models;

namespace starskycore.ViewModels
{
    public class ArchiveViewModel
    {
        public IEnumerable<FileIndexItem> FileIndexItems { get; set; }
        public List<string> Breadcrumb { get; set; }
        public RelativeObjects RelativeObjects { get; set; }
        public string SearchQuery { get; set; }
        
        // Used PageType by react client
        public string PageType => PageViewType.PageType.Archive.ToString();
        public string SubPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; } = true;
        
        /// <summary>
        /// Count the number of files (collection setting is ignored for this value)
        /// </summary>
        public int CollectionsCount { get; set; }
        public bool IsReadOnly { get; set; }
        
        /// <summary>
        /// Display only the current filter selection of ColorClasses
        /// </summary>
        public List<FileIndexItem.Color> ColorClassFilterList { get; set; }

        /// <summary>
        /// Give back a list of all colorClasses that are used in this specific folder 
        /// </summary>
        public List<FileIndexItem.Color> ColorClassUsage { get; set; }

    }
}
