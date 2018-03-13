using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<ObjectItem> ObjectItems { get; set; }
        public List<string> Breadcrumb { get; set; }
        public ObjectItem SingleItem { get; set; }
    }
}
