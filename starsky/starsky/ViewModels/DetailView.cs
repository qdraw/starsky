using System.Collections.Generic;
using starsky.Models;

namespace starsky.ViewModels
{
    public class DetailView
    {
        public FileIndexItem FileIndexItem { get; set; }
        public RelativeObjects RelativeObjects { get; set; }
        public List<string> Breadcrumb { get; set; }
        public IEnumerable<FileIndexItem.ColorUserInterface> GetAllColor { get; set; }
        public List<FileIndexItem.Color> ColorClassFilterList { get; set; }
        // Used by react client
        public string PageType => PageViewType.PageType.DetailView.ToString();
        public bool IsDirectory { get; set; }
    }
}
