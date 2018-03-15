using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.ViewModels
{
    public class SearchViewModel
    {
        public IEnumerable<ObjectItem> ObjectItems { get; set; }
        public List<string> Breadcrumb { get; set; }
        public string SearchQuery { get; set; }
        public int PageNumber { get; set; }
    }
}
