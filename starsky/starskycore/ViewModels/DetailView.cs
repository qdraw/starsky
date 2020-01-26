using System.Collections.Generic;
#if NETSTANDARD2_1
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif
using starskycore.Models;

namespace starskycore.ViewModels
{
    public class DetailView
    {
        public FileIndexItem FileIndexItem { get; set; }

        public List<string> Breadcrumb { get; set; }
        
	    /// <summary>
	    /// List of selected Color Class's
	    /// </summary>
	    public List<FileIndexItem.Color> ColorClassActiveList { get; set; } = new List<FileIndexItem.Color>();
        
	    // Used by react client
        public string PageType => PageViewType.PageType.DetailView.ToString();
        
        // To return error codes// in the json it is always false
        public bool IsDirectory { get; set; }
        
        public string SubPath { get; set; }

	    /// <summary>
	    /// Is collections enabled?
	    /// </summary>
	    public bool Collections { get; set; }

	    /// <summary>
	    /// If collections is enabled return list of subpaths
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
	    
	    /// <summary>
	    /// Private field for next/prev
	    /// </summary>
	    private RelativeObjects _relativeObjects;

	    public RelativeObjects RelativeObjects
	    {
		    get => _relativeObjects;
		    set
		    {
			    _relativeObjects = new RelativeObjects(Collections, ColorClassActiveList);
			    _relativeObjects = value;
		    }
	    }

    }
}
