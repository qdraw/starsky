using System.Collections.Generic;
using Newtonsoft.Json;
using starsky.Models;

namespace starsky.ViewModels
{
    public class DetailView
    {
        public FileIndexItem FileIndexItem { get; set; }
        public RelativeObjects RelativeObjects { get; set; }
        public List<string> Breadcrumb { get; set; }
        public List<FileIndexItem.Color> ColorClassFilterList { get; set; }
        // Used by react client
        public string PageType => PageViewType.PageType.DetailView.ToString();
        // To return error codes// in the json it is always false
        public bool IsDirectory { get; set; }

        // Used by Razor view
        [JsonIgnore]
        public IEnumerable<FileIndexItem.ColorUserInterface> GetAllColor { get; set; }
    }
}
