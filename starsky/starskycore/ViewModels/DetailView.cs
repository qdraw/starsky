using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using starsky.ViewModels;
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
	    public List<FileIndexItem.Color> ColorClassFilterList { get; set; } = new List<FileIndexItem.Color>();
        
	    // Used by react client
        public string PageType => PageViewType.PageType.DetailView.ToString();
        // To return error codes// in the json it is always false
        public bool IsDirectory { get; set; }
        
        public string SubPath { get; set; }

        // Used by Razor view
        [JsonIgnore]
        public IEnumerable<FileIndexItem.ColorUserInterface> GetAllColor { get; set; }

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
	    
	    /// <summary>
	    /// Get the next/prev item with Args used
	    /// </summary>
	    public RelativeObjects RelativeObjects
	    {
		    get
		    {
			    var urlRelative = new RelativeObjects
			    {
				    NextFilePath = _relativeObjects.NextFilePath,
				    PrevFilePath = _relativeObjects.PrevFilePath
			    };

			    if ( !Collections )
			    {
				    urlRelative.Args.Add(nameof(Collections).ToLowerInvariant(),Collections.ToString().ToLowerInvariant());
			    }
			    
			    if (ColorClassFilterList != null && ColorClassFilterList.Count >= 1 )
			    {
				    var colorClassArg = new StringBuilder();
				    for ( int i = 0; i < ColorClassFilterList.Count; i++ )
				    {
					    var colorClass = ColorClassFilterList[i];
					    if (i ==  ColorClassFilterList.Count-1)
					    {
						    colorClassArg.Append(colorClass.GetHashCode());
					    }
					    else
					    {
						    colorClassArg.Append(colorClass.GetHashCode()+ ",");
					    }
				    }
				    urlRelative.Args.Add(nameof(FileIndexItem.ColorClass).ToLowerInvariant(),colorClassArg.ToString());
			    }
			    return urlRelative;
		    }
		    set { _relativeObjects = value; }
	    }

	    
    }
}
