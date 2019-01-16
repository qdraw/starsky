using System.Collections.Generic;
using Newtonsoft.Json;
using starsky.Models;
using starskycore.Models;

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
        
        public string SubPath { get; set; }

        // Used by Razor view
        [JsonIgnore]
        public IEnumerable<FileIndexItem.ColorUserInterface> GetAllColor { get; set; }
	    
	    /// <summary>
	    /// If conllections enalbed return list of subpaths
	    /// </summary>
	    /// <param name="detailView">the base fileIndexItem</param>
	    /// <param name="collections">bool, to enable</param>
	    /// <param name="subPath">the file orginal requested in subpath style</param>
	    /// <returns></returns>
	    public List<string> GetCollectionSubPathList(DetailView detailView, bool collections, string subPath)
	    {
		    // Paths that are used
		    var collectionSubPathList = detailView.FileIndexItem.CollectionPaths;
		    // when not running in collections mode only update one file
		    if (!collections) collectionSubPathList = new List<string> {subPath};
		    return collectionSubPathList;
	    }
	    
    }
}
