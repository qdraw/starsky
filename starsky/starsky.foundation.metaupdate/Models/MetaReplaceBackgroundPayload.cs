namespace starsky.foundation.metaupdate.Models;

public sealed class MetaReplaceBackgroundPayload
{
	/// <summary>
	/// All subPaths that need to be updated should be in this list.
	///  So if Collections is true, then it will be all subPaths
	/// </summary>
	public List<string> SubPaths { get; set; } = [];
}
