namespace starsky.feature.webftppublish.FtpAbstractions.Interfaces
{
	public interface IFtpWebRequestFactory
	{
		IFtpWebRequest Create(string uri);
	}
}
