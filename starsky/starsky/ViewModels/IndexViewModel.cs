using System.Collections.Generic;
using starsky.Models;

namespace starsky.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<FileIndexItem> FileIndexItems { get; set; }
        public List<string> Breadcrumb { get; set; }
        public RelativeObjects RelativeObjects { get; set; }
        public string SearchQuery { get; set; }
//        public IEnumerable<FileIndexItem.ColorUserInterface> GetAllColor { get; set; }
    }
}
