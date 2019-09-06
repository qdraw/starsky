using System.Collections.Generic;
using System.Text;
using starskycore.Models;

namespace starskycore.ViewModels
{
	/// <summary>
	/// In Detailview there are values added for handeling Args  
	/// </summary>
    public class RelativeObjects
    {
        public string NextFilePath { get; set; }
        public string PrevFilePath { get; set; }

        public string NextHash { get; set; }

        public string PrevHash { get; set; }
        
        /// <summary>
        /// Private field
        /// </summary>
	    private Dictionary<string,string> ArgsPrivate { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Prevent overwrites with null args
        /// </summary>
	    public Dictionary<string,string>  Args
	    {
		    get { return ArgsPrivate; }
		    set
		    {
			    if ( value == null ) return;
			    ArgsPrivate = value;
		    }
	    }
	    
	    public RelativeObjects()
	    {
	    }

	    /// <summary>
	    /// Set Args based on Collections and ColorClass Settings
	    /// </summary>
	    /// <param name="Collections"></param>
	    /// <param name="ColorClassFilterList"></param>
	    public RelativeObjects(bool Collections, List<FileIndexItem.Color> ColorClassFilterList)
	    {
		    	if ( !Collections )
			    {
				    Args.Add(nameof(Collections).ToLowerInvariant(),Collections.ToString().ToLowerInvariant());
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
				    Args.Add(nameof(FileIndexItem.ColorClass).ToLowerInvariant(),colorClassArg.ToString());
			    }
	    }
    }
}
