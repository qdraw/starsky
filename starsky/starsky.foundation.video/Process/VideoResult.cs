namespace starsky.foundation.video.Process;

public class VideoResult
{
	public VideoResult(bool? isSuccess = null, string? resultPath = null,
		string? errorMessage = null)
	{
		if ( isSuccess != null )
		{
			IsSuccess = ( bool ) isSuccess;
		}

		ResultPath = resultPath;
		ErrorMessage = errorMessage;
	}

	public bool IsSuccess { get; set; }

	public string? ErrorMessage { get; set; }

	public string? ResultPath { get; set; }
}
