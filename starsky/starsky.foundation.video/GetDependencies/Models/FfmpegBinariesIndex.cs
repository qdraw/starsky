namespace starsky.foundation.video.GetDependencies.Models;

public class BinaryIndex
{
    public string Architecture { get; set; }
    public string Url { get; set; }
    public string Sha256 { get; set; }
}

public class FfmpegBinariesIndex
{
    public List<BinaryIndex> Binaries { get; set; }
}

public class FfmpegBinariesContainer {

	public FfmpegBinariesContainer(Uri? indexUrl, bool success, FfmpegBinariesIndex? data)
	{
		Data = data;
		IndexUrl = indexUrl;
		Success = success;
	}
	
	public bool Success { get; set; }
	public Uri? IndexUrl { get; set; }
	public FfmpegBinariesIndex? Data  { get; set; }
}
