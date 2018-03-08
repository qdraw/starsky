using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.Models
{
    public class FileIndexItem
    {
        public int Id { get; set; }

        [Column(Order = 2)]
        public string FilePath { get; set; }

        [Column(Order = 1)]
        public string FileName { get; set; }

    }
}
