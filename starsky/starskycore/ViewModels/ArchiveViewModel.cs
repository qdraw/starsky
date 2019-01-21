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
        public int CollectionsCount { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
