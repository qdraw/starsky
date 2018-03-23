using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.Models;

namespace starsky.ViewModels
{
    public class DetailView
    {
        public FileIndexItem FileIndexItem { get; set; }
        public RelativeObjects RelativeObjects { get; set; }
        public List<string> Breadcrumb { get; set; }
        public IEnumerable<FileIndexItem.Color> GetAllColor { get; set; }

    }
}
