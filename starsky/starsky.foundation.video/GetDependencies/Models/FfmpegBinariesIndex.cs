namespace starsky.foundation.video.GetDependencies.Models;

public class BinaryIndex
{
    public string Architecture { get; set; }
    public string Url { get; set; }
    public string Sha256 { get; set; }
}

public class FfmpegBinariesIndex
{
	public bool Success { get; set; } = true;
    public List<BinaryIndex> Binaries { get; set; }
}
