namespace starsky.foundation.platform.Helpers
{
	public static class JsonSidecarLocation
	{
		public static string JsonLocation(string subPath)
		{
			var fileName = PathHelper.GetFileName(subPath);
			var parentDirectory = subPath.Replace(fileName, string.Empty);
			return JsonLocation(parentDirectory, fileName);
		}
		
		/// <summary>
		/// Get the jsonSubPath `parentDir/.starsky.filename.ext.json`
		/// </summary>
		/// <param name="parentDirectory">parent Directory</param>
		/// <param name="fileName">and filename</param>
		/// <returns>parentDir/.starsky.filename.`ext.json</returns>
		public static string JsonLocation(string parentDirectory, string fileName)
		{
			return PathHelper.AddSlash(parentDirectory) + ".starsky." + fileName
			       + ".json";
		}
	}
}
