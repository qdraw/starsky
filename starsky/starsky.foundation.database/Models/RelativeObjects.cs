using System.Collections.Generic;
using System.Text;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Models
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
	    /// <param name="collections"></param>
	    /// <param name="colorClassActiveList"></param>
	    public RelativeObjects(bool collections, List<ColorClassParser.Color> colorClassActiveList)
	    {
		    	if ( !collections )
			    {
				    Args.Add(nameof(collections).ToLowerInvariant(),"false");
			    }
			    
			    if (colorClassActiveList != null && colorClassActiveList.Count >= 1 )
			    {
				    var colorClassArg = new StringBuilder();
				    for ( int i = 0; i < colorClassActiveList.Count; i++ )
				    {
					    var colorClass = colorClassActiveList[i];
					    if (i ==  colorClassActiveList.Count-1)
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
