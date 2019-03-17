using System.Collections.Generic;

namespace starskycore.ViewModels
{
    public class RelativeObjects
    {
        public string NextFilePath { get; set; }
        public string PrevFilePath { get; set; } 
	    public Dictionary<string,string> Args { get; set; } = new Dictionary<string, string>();
    }
}
